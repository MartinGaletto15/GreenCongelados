using Application.Interfaces.Order;
using Application.Interfaces.User;
using Application.Interfaces.UserServices;
using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;
using Domain.Entities.Enums;

namespace Application.Services;

public class CadetService : ICadetReadOnlyService, ICadetWriteService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderWriteService _orderWriteService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserReadOnlyService _userReadOnlyService;
    
    public CadetService(
        IOrderRepository orderRepository, 
        IOrderWriteService orderWriteService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork, 
        IUserReadOnlyService userReadOnlyService)
    {
        _orderRepository = orderRepository;
        _orderWriteService = orderWriteService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userReadOnlyService = userReadOnlyService;
    }

    public async Task<IEnumerable<UserDTO>> GetAllCadetsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var cadets = users.Where(u => u.Role == Role.CADET);

        return UserDTO.Create(cadets);
    }

    public async Task<UserDTO> GetCadetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != Role.CADET)
            throw new AppValidationException("El cadete no existe o el usuario no es un cadete.", "CADET_NOT_FOUND", 404);
            
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> AsingOrderToCadetAsync(int id, int idOrder)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != Role.CADET)
            throw new AppValidationException("El cadete no existe o el usuario no es un cadete.", "CADET_NOT_FOUND", 404);

        var order = await _orderRepository.GetByIdAsync(idOrder);
        if (order == null)
            throw new AppValidationException("La orden no existe.", "ORDER_NOT_FOUND", 404);
            
        order.AssignCourier(id);
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        return UserDTO.Create(user);
    }

    public async Task<OrderDTO> UpdateOrderStatusAsync(int idCadet, int idOrder, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(idOrder);
        if (order == null)
            throw new AppValidationException("La orden no existe.", "ORDER_NOT_FOUND", 404);

        if (order.IdCourier != idCadet)
            throw new AppValidationException("No tienes permiso para actualizar esta orden.", "FORBIDDEN", 403);

        // Only the customer or an admin can cancel an order. If delivery fails, the cadet
        // must return it to LogisticsPending so it can be reassigned.
        if (status == OrderStatus.Cancelled)
            throw new AppValidationException("Un cadete no puede cancelar una orden; debe devolverla a pendiente de logística.", "FORBIDDEN", 403);

        // Reuse the order workflow so stock restoration on cancellation stays centralized.
        return await _orderWriteService.UpdateOrderStatusAsync(idOrder, status);
    }
}
