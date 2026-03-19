using Aplication.Interfaces.OrderItem;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class OrderItemService : IOrderItemService
{
    private readonly IGenericRepository<OrderItem> _repository;

    public OrderItemService(IGenericRepository<OrderItem> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderItemDTO>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return OrderItemDTO.Create(entities);
    }

    public async Task<OrderItemDTO> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("OrderItem not found", "ORDERITEM_NOT_FOUND");
        return OrderItemDTO.Create(entity);
    }

    public async Task<OrderItemDTO> CreateAsync(CreateOrderItemRequest request)
    {
        var entity = new OrderItem
        {
            IdOrder = request.IdOrder,
            Order = null!,
            IdProduct = request.IdProduct,
            Product = null!,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        await _repository.AddAsync(entity);
        return OrderItemDTO.Create(entity);
    }

    public async Task<OrderItemDTO> UpdateAsync(int id, UpdateOrderItemRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("OrderItem not found", "ORDERITEM_NOT_FOUND");

        entity.Quantity = request.Quantity ?? entity.Quantity;
        entity.UnitPrice = request.UnitPrice ?? entity.UnitPrice;

        await _repository.UpdateAsync(entity);
        return OrderItemDTO.Create(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new AppValidationException("OrderItem not found", "ORDERITEM_NOT_FOUND");
        await _repository.DeleteAsync(entity);
    }
}