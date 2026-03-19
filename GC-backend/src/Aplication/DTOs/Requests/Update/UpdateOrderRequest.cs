using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record UpdateOrderRequest(
    string? ShippingAddress,
    decimal? ShippingCost,
    OrderStatus? OrderStatus
);