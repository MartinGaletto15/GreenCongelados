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
using Domain.Interfaces;
using Domain.Exceptions;
using Domain.Enums.Entities;

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
    private readonly IOrderItemWriteService _orderItemService;
    private readonly IProductReadOnlyService _productService;
    private readonly IUserReadOnlyService _userService;
    private readonly IAddressReadOnlyService _addressService;

    public OrderService(
        IOrderRepository repository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IPromotionReadOnlyService promotionService,
        IShippingCostReadOnlyService shippingCostService,
        ICartItemReadOnlyService cartItemReadOnlyService,
        ICartItemWriteService cartItemWriteService,
        IOrderItemWriteService orderItemService,
        IProductReadOnlyService productService,
        IUserReadOnlyService userService,
        IAddressReadOnlyService addressService)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _promotionService = promotionService;
        _shippingCostService = shippingCostService;
        _cartItemReadOnlyService = cartItemReadOnlyService;
        _cartItemWriteService = cartItemWriteService;
        _orderItemService = orderItemService;
        _productService = productService;
        _userService = userService;
        _addressService = addressService;
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
        
        // 1. Fetch Cart Items
        var cartItems = await _cartItemReadOnlyService.GetCartItemsAsync(userId);
        if (!cartItems.Any()) throw new AppValidationException("Cart is empty", "EMPTY_CART");

        // 2. Fetch Products and Validate Stock
        var productsToUpdate = new List<(Product Entity, int Quantity)>();
        decimal subtotal = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null) throw new AppValidationException($"Product {cartItem.ProductId} not found", "PRODUCT_NOT_FOUND");
            
            if (product.CurrentStock < cartItem.Quantity)
                throw new AppValidationException($"Insufficient stock for product: {product.Name}. Available: {product.CurrentStock}", "INSUFFICIENT_STOCK");

            subtotal += product.Price * cartItem.Quantity;
            productsToUpdate.Add((product, cartItem.Quantity));
        }

        // 3. Handle Promotion
        var promotion = request.CouponCode != null 
            ? await _promotionService.GetByCodeAsync(request.CouponCode) 
            : null;

        decimal discount = 0;
        decimal shippingDiscount = 1;
        
        if (promotion != null)
        {
            var now = DateTime.Now;
            if (now < promotion.StartDate || now > promotion.EndDate)
                throw new AppValidationException("The promotion coupon has expired or is not yet valid", "PROMOTION_EXPIRED");

            if (promotion.MinAmount.HasValue && subtotal < promotion.MinAmount.Value)
                throw new AppValidationException($"Min amount for promotion is {promotion.MinAmount.Value}", "PROMOTION_MIN_AMOUNT");

            if (promotion.DiscountType == "Percentage")
                discount = subtotal * (promotion.DiscountValue / 100);
            else if (promotion.DiscountType == "FixedAmount")
                discount = promotion.DiscountValue;
            else if (promotion.DiscountType == "FreeShipping")
                shippingDiscount = 0;
        }

        // 4. Handle Shipping
        var shippingCost = await _shippingCostService.GetActiveAsync();
        decimal shippingAmount = shippingCost?.Cost ?? 0;

        // 5. TRANSACTIONAL BLOCK
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 5.1 Create Order
            var entity = new Order
            {
                IdUser = userId,
                User = null!,
                IdCourier = null,
                IdPromotion = promotion?.IdPromotion,
                ShippingCost = shippingAmount * shippingDiscount,
                ShippingStreet = address.Street,
                ShippingDpto = address.Dpto,
                ShippingReference = address.References,
                OrderDate = DateTime.Now,
                GlobalDiscount = discount,
                Subtotal = subtotal,
                Total = subtotal + (shippingAmount * shippingDiscount) - discount,
                OrderStatus = OrderStatus.LogisticsPending
            };

            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // To get IdOrder

            // 5.2 Create OrderItems and Update Stock
            foreach (var item in productsToUpdate)
            {
                // Create Item
                var orderItem = new OrderItem
                {
                    IdOrder = entity.IdOrder,
                    IdProduct = item.Entity.IdProduct,
                    Quantity = item.Quantity,
                    UnitPrice = item.Entity.Price
                };
                
                item.Entity.CurrentStock -= item.Quantity;
                await _productRepository.UpdateAsync(item.Entity);
                
                entity.OrderItems.Add(orderItem);
            }

            // 5.3 Clear Cart
            foreach (var cartItem in cartItems)
            {
                await _cartItemWriteService.DeleteCartItemAsync(userId, cartItem.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var savedEntity = await _repository.GetByIdWithDetailsAsync(entity.IdOrder);
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
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        if (entity.IdUser != userId)
            throw new AppValidationException("Not authorized to update this order", "FORBIDDEN");

        if (entity.OrderStatus == OrderStatus.Delivered)
            throw new AppValidationException("Cannot update a delivered order", "INVALID_OPERATION");

        entity.ShippingStreet = request.ShippingStreet ?? entity.ShippingStreet;
        entity.ShippingDpto = request.ShippingDpto ?? entity.ShippingDpto;
        entity.ShippingReference = request.ShippingReference ?? entity.ShippingReference;
        entity.ShippingCost = request.ShippingCost ?? entity.ShippingCost;
        entity.OrderStatus = request.OrderStatus ?? entity.OrderStatus;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return OrderDTO.Create(entity);
    }

    public async Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        entity.OrderStatus = status;
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return OrderDTO.Create(entity);
    }

    public async Task<OrderDTO> DeleteAsync(int id, int userId, string role)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Order not found", "ORDER_NOT_FOUND");

        if (entity.IdUser != userId && role != "ADMIN" && role != "SUPERADMIN")
            throw new AppValidationException("Not authorized to delete/cancel this order", "FORBIDDEN");

        entity.OrderStatus = OrderStatus.Cancelled;
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return OrderDTO.Create(entity);
    }
}
