using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdateOrderRequestAdmin (
    int? IdCourier,
    OrderStatus? OrderStatus
);