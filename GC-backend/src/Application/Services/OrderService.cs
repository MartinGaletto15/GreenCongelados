using Application.Interfaces.Order;
using Application.Interfaces.Promotion;
using Application.Interfaces.ShippingCost;
using Application.Interfaces.Address;
using Application.Interfaces.CartItem;
using Application.Interfaces.OrderItem;
using Application.Interfaces.Product;
using Application.Interfaces.UserServices;
using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Application.Services;

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
    private readonly IPromotionRepository _promotionRepository;

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
        IPromotionRepository promotionRepository)
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

    public async Task<OrderDTO> GetOrderByIdAsync(int id, int userId, Role role)
    {
        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);

        var isAdmin = Roles.IsAdmin(role);
        var isOwner = entity.IdUser == userId;
        var isAssignedCourier = entity.IdCourier.HasValue && entity.IdCourier == userId;

        if (!isAdmin && !isOwner && !isAssignedCourier)
            throw new AppValidationException("No tienes permiso para ver esta orden.", "FORBIDDEN", 403);

        return OrderDTO.Create(entity);
    }

    public async Task<IEnumerable<OrderDTO>> GetMyOrdersAsync(int userId)
    {
        var entities = await _repository.GetByUserIdWithDetailsAsync(userId);
        return OrderDTO.Create(entities);
    }

    public async Task<OrderDTO> GetMyOrderByIdAsync(int id, int userId, Role role)
    {
        return await GetOrderByIdAsync(id, userId, role);
    }

    public async Task<OrderDTO> CreateAsync(CreateOrderRequest request, int userId)
    {
        await _userService.GetUserByIdAsync(userId);
        var address = await _addressService.GetByUserIdAsync(userId);

        var cartItems = (await _cartItemReadOnlyService.GetCartItemsAsync(userId)).ToList();
        if (cartItems.Count == 0) throw new AppValidationException("El carrito está vacío.", "EMPTY_CART");

        // Lectura NO trackeada: solo para validar temprano y dar buenos mensajes.
        var items = new List<(int IdProduct, decimal UnitPrice, int Quantity, string Name)>();
        foreach (var cartItem in cartItems)
        {
            var product = await _productRepository.GetByIdReadOnlyAsync(cartItem.ProductId);
            if (product == null || product.Status == ProductStatus.Deleted)
                throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);
            if (product.Status != ProductStatus.Available)
                throw new AppValidationException($"El producto no está disponible: {product.Name}.", "PRODUCT_NOT_AVAILABLE");
            if (product.CurrentStock < cartItem.Quantity)
                throw new AppValidationException($"Stock insuficiente para el producto: {product.Name}. Disponible: {product.CurrentStock}.", "INSUFFICIENT_STOCK");

            items.Add((product.IdProduct, product.Price, cartItem.Quantity, product.Name));
        }

        var normalizedCouponCode = request.CouponCode is not null
            ? Promotion.NormalizeCouponCode(request.CouponCode) : null;
        var promotion = normalizedCouponCode != null
            ? (await _promotionRepository.GetAllAsync()).FirstOrDefault(x => x.CouponCode == normalizedCouponCode) : null;

        var shippingCost = await _shippingCostService.GetActiveAsync();
        decimal baseShippingAmount = shippingCost?.Cost ?? 0;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Ordenar por IdProduct evita deadlocks entre pedidos con 2+ productos iguales en orden inverso.
            foreach (var item in items.OrderBy(x => x.IdProduct))
                await EnsureStockOrThrowAsync(item);

            var order = Order.Create(
                userId,
                address.Street,
                address.Dpto,
                address.References,
                baseShippingAmount,
                items.Select(x => (x.IdProduct, x.UnitPrice, x.Quantity)),
                promotion);

            await _repository.AddAsync(order);

            // Una sola sentencia, sin SaveChanges por item.
            await _cartItemWriteService.ClearCartAsync(userId);

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
        if (entity == null) throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);

        if (entity.IdUser != userId)
            throw new AppValidationException("No tienes permiso para actualizar esta orden.", "FORBIDDEN", 403);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            entity.UpdateShippingDetails(request.ShippingStreet, request.ShippingDpto, request.ShippingReference);

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
        if (status == OrderStatus.Cancelled)
            return await CancelOrderAsync(id);

        var entity = await _repository.GetByIdWithDetailsAsync(id);
        if (entity == null) throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);

        entity.UpdateStatus(status);          // mantiene las transiciones no-cancelables
        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return OrderDTO.Create(entity);
    }

    public async Task<OrderDTO> DeleteAsync(int id, int userId, Role role)
    {
        var order = await _repository.GetByIdWithDetailsNoTrackingAsync(id);
        if (order == null) throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);
        if (order.IdUser != userId && !Roles.IsAdmin(role))
            throw new AppValidationException("No tienes permiso para cancelar esta orden.", "FORBIDDEN", 403);

        return await CancelOrderAsync(id);
    }

    private async Task EnsureStockOrThrowAsync((int IdProduct, decimal UnitPrice, int Quantity, string Name) item)
    {
        if (await _productRepository.TryDecreaseStockAsync(item.IdProduct, item.Quantity))
            return;

        var current = await _productRepository.GetByIdReadOnlyAsync(item.IdProduct);
        if (current is null || current.Status == ProductStatus.Deleted)
            throw new AppValidationException("Producto no encontrado.", "PRODUCT_NOT_FOUND", 404);
        if (current.Status != ProductStatus.Available)
            throw new AppValidationException($"El producto no está disponible: {current.Name}.", "PRODUCT_NOT_AVAILABLE", 409);

        throw new AppValidationException($"Stock insuficiente para el producto: {current.Name}. Disponible: {current.CurrentStock}.", "INSUFFICIENT_STOCK", 409);
    }

    private async Task<OrderDTO> CancelOrderAsync(int id)
    {
        // Snapshot solo para saber qué devolver al stock.
        var snapshot = await _repository.GetByIdWithDetailsNoTrackingAsync(id);
        if (snapshot == null) throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);
        if (snapshot.OrderStatus == OrderStatus.Delivered)
            throw new AppValidationException("No se puede cancelar una orden entregada.", "INVALID_STATUS_TRANSITION", 409);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!await _repository.TryCancelAsync(id))
                throw new AppValidationException("La orden ya fue cancelada.", "ALREADY_CANCELLED", 409);

            foreach (var item in snapshot.OrderItems)
                await _productRepository.IncreaseStockAsync(item.IdProduct, item.Quantity);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var fresh = await _repository.GetByIdWithDetailsNoTrackingAsync(id);
            if (fresh is null)
                throw new AppValidationException("Orden no encontrada.", "ORDER_NOT_FOUND", 404);
            return OrderDTO.Create(fresh);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
