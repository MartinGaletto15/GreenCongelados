using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Applications.dtos.Requests;

public record UpdateOrderRequest(
    string? ShippingStreet,
    string? ShippingDpto,
    string? ShippingReference,
    decimal? ShippingCost,
    OrderStatus? OrderStatus
);
