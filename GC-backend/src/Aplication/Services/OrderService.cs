using System.Security.Claims;
using Aplication.Interfaces.Order;
using Aplication.Interfaces.Promotion;
using Aplication.Interfaces.ShippingCost;
using Aplication.Interfaces.Address;
using Aplication.Interfaces.CartItem;
using Aplication.Interfaces.OrderItem;
using Aplication.Interfaces.Product;
using Aplication.Interfaces.UserServices;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class OrderService : IOrderReadOnlyService, IOrderWriteService
{
    private readonly IOrderRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPromotionReadOnlyService _promotionService;
    private readonly IShippingCostReadOnlyService _shippingCostService;
    private readonly ICartItemReadOnlyService _cartItemReadOnlyService;
    private readonly ICartItemWriteService _cartItemWriteService;
    private readonly IUserReadOnlyService _userService;
    private readonly IAddressReadOnlyService _addressService;
    private readonly IGenericRepository<Promotion> _promotionRepository;

    public OrderService(
        IOrderRepository repository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IPromotionReadOnlyService promotionService,
        IShippingCostReadOnlyService shippingCostService,
        ICartItemReadOnlyService cartItemReadOnlyService,
        ICartItemWriteService cartItemWriteService,
        IUserReadOnlyService userService,
        IAddressReadOnlyService addressService,
        IGenericRepository<Promotion> promotionRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _promotionService = promotionService;
        _shippingCostService = shippingCostService;
        _cartItemReadOnlyService = cartItemReadOnlyService;
        _cartItemWriteService = cartItemWriteService;
        _userService = userService;
        _addressService = addressService;
        _promotionRepository = promotionRepository;
    }

    public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
    {
        var entities = await _repository.GetAllWithDetailsAsync();
        return OrderDTO.Create(entities);
    }

    public async Task<IEnumerable<OrderDTO>> GetOrdersByCourierAsync(int courierId)
    {
        var entities = await _repository.GetByCourierIdWithDetailsAsync(courierId);
        return OrderDTO.Create(entities);
    }

    public async Task<OrderDTO> GetOrderByIdAsync(int id, int userId, string role)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        if (entity.IdUser != userId && role != "ADMIN" && role != "SUPERADMIN" && role != "CADET")
            throw new AppValidationException("Not authorized to view this order", "FORBIDDEN");

        return OrderDTO.Create(entity);
    }

    public async Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(int userId)
    {
        var entities = await _repository.GetByUserIdWithDetailsAsync(userId);
        return OrderDTO.Create(entities);
    }

    public async Task<OrderDTO> GetMyOrderByIdAsync(int id, int userId, string role)
    {
        return await GetOrderByIdAsync(id, userId, role);
    }

    public async Task<OrderDTO> CreateAsync(CreateOrderRequest request, int userId)
    {
        await _userService.GetUserByIdAsync(userId);
        var address = await _addressService.GetByUserIdAsync(userId);
        
        var cartItems = await _cartItemReadOnlyService.GetCartItemsAsync(userId);
        if (!cartItems.Any()) throw new AppValidationException("Cart is empty", "EMPTY_CART");

        var productsWithQuantity = new List<(Product Product, int Quantity)>();
        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null) throw new AppValidationException($"Product {cartItem.ProductId} not found", "PRODUCT_NOT_FOUND");
            productsWithQuantity.Add((product, cartItem.Quantity));
        }

        var promotion = request.CouponCode != null 
            ? (await _promotionRepository.GetAllAsync()).FirstOrDefault(x => x.CouponCode == request.CouponCode)
            : null;

        var shippingCost = await _shippingCostService.GetActiveAsync();
        decimal baseShippingAmount = shippingCost?.Cost ?? 0;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // The logic for stock validation, calculation and items creation is now in the Order.Create domain method
            var order = Order.Create(
                userId,
                address.Street,
                address.Dpto,
                address.References,
                baseShippingAmount,
                productsWithQuantity,
                promotion
            );

            await _repository.AddAsync(order);
            
            // Clear Cart
            foreach (var cartItem in cartItems)
            {
                await _cartItemWriteService.DeleteCartItemAsync(userId, cartItem.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var savedEntity = await _repository.GetByIdWithDetailsAsync(order.IdOrder);
            return OrderDTO.Create(savedEntity!);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDTO> UpdateMyOrderAsync(int id, UpdateOrderRequest request, int userId)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        if (entity.IdUser != userId)
            throw new AppValidationException("Not authorized to update this order", "FORBIDDEN");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            entity.UpdateShippingDetails(request.ShippingStreet, request.ShippingDpto, request.ShippingReference);
            
            if (request.OrderStatus.HasValue && request.OrderStatus.Value != entity.OrderStatus)
            {
                if (request.OrderStatus.Value == OrderStatus.Cancelled)
                    await ExecuteCancellationAsync(entity);
                else
                    entity.UpdateStatus(request.OrderStatus.Value);
            }

            await _repository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return OrderDTO.Create(entity);
    }

    public async Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        // Use rich domain methods for status transitions
        switch (status)
        {
            case OrderStatus.Cancelled:
                if (entity.OrderStatus != OrderStatus.Cancelled)
                {
                    await ExecuteCancellationAsync(entity);
                }
                break;
            case OrderStatus.Delivered:
                entity.MarkAsDelivered();
                await _repository.UpdateAsync(entity);
                break;
            default:
                entity.UpdateStatus(status);
                await _repository.UpdateAsync(entity);
                break;
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        return OrderDTO.Create(entity);
    }

    public async Task<OrderDTO> DeleteAsync(int id, int userId, string role)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        if (entity.IdUser != userId && role != "ADMIN" && role != "SUPERADMIN")
            throw new AppValidationException("Not authorized to delete/cancel this order", "FORBIDDEN");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (entity.OrderStatus != OrderStatus.Cancelled)
            {
                await ExecuteCancellationAsync(entity);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        return OrderDTO.Create(entity);
    }

    private async Task ExecuteCancellationAsync(Order order)
    {
        order.Cancel();
        await RestoreStockAsync(order);
        await _repository.UpdateAsync(order);
    }

    private async Task RestoreStockAsync(Order order)
    {
        foreach (var item in order.OrderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.IdProduct);
            if (product != null)
            {
                product.IncreaseStock(item.Quantity);
                await _productRepository.UpdateAsync(product);
            }
        }
    }
}
