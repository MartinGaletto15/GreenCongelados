using Domain.Entities.Enums;

namespace Applications.dtos.Requests;

public record UpdateOrderRequestAdmin (
    int? IdCourier,
    OrderStatus? OrderStatus
);