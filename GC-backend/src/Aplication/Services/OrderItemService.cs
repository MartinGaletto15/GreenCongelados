using Aplication.Interfaces.OrderItem;
using Applications.dtos;
using Applications.dtos.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;

namespace Aplication.Services;

public class OrderItemService : IOrderItemReadOnlyService, IOrderItemWriteService
{
    private readonly IOrderItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderItemService(IOrderItemRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrderItemDTO>> GetByOrderIdAsync(int orderId)
    {
        var entities = await _repository.GetByOrderIdAsync(orderId);
        return OrderItemDTO.Create(entities);
    }

    public async Task<OrderItemDTO> CreateAsync(CreateOrderItemRequest request)
    {
        var entity = new OrderItem(request.IdProduct, request.Quantity, request.UnitPrice, request.IdOrder);

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return OrderItemDTO.Create(entity);
    }
}