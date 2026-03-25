using Aplication.Interfaces.User;
using Aplication.Interfaces.UserServices;
using Applications.dtos;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Exceptions;
using Domain.Entities.Enums;

namespace Aplication.Services;

public class CadetService : ICadetReadOnlyService, ICadetWriteService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserReadOnlyService _userReadOnlyService;
    
    public CadetService(
        IOrderRepository orderRepository, 
        IUserRepository userRepository,
        IUnitOfWork unitOfWork, 
        IUserReadOnlyService userReadOnlyService)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userReadOnlyService = userReadOnlyService;
    }

    public async Task<IEnumerable<UserDTO>> GetAllCadetsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var cadets = users.Where(u => u.Role == Role.CADET);
        
        if (!cadets.Any())
            throw new AppValidationException("No hay cadetes registrados en el sistema", "CADET_LIST_EMPTY");

        return UserDTO.Create(cadets);
    }

    public async Task<UserDTO> GetCadetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != Role.CADET)
            throw new AppValidationException("El cadete no existe o el usuario no es un cadete", "CADET_NOT_FOUND");
            
        return UserDTO.Create(user);
    }

    public async Task<UserDTO> AsingOrderToCadetAsync(int id, int idOrder)
    {
        // 1. Validar cadete
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Role != Role.CADET)
            throw new AppValidationException("El cadete no existe o el usuario no es un cadete", "CADET_NOT_FOUND");

        // 2. Buscar la orden usando el repositorio de órdenes
        var order = await _orderRepository.GetByIdAsync(idOrder);
        if (order == null)
            throw new AppValidationException("La orden no existe", "ORDER_NOT_FOUND");

        // 3. Asignar cadete a la orden
        order.AssignCourier(id);
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        return UserDTO.Create(user);
    }

    public async Task<OrderDTO> UpdateOrderStatusAsync(int idCadet, int idOrder, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(idOrder);
        if (order == null)
            throw new AppValidationException("La orden no existe", "ORDER_NOT_FOUND");

        if (order.IdCourier != idCadet)
            throw new AppValidationException("No tienes permiso para actualizar esta orden", "FORBIDDEN");

        order.UpdateStatus(status);
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return OrderDTO.Create(order);
    }
}