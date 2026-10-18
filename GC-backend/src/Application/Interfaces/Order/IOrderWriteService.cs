using Application.DTOs;
using Application.DTOs.Requests;
using Domain.Entities.Enums;

namespace Application.Interfaces.Order;

public interface IOrderWriteService
{
    Task<OrderDTO> CreateAsync(CreateOrderRequest request, int userId);
    Task<OrderDTO> UpdateMyOrderAsync(int id, UpdateOrderRequest request, int userId);
    Task<OrderDTO> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task<OrderDTO> DeleteAsync(int id, int userId, string role);
}