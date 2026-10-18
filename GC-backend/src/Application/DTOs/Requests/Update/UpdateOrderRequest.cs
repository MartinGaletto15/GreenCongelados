using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdateOrderRequest(
    string? ShippingStreet,
    string? ShippingDpto,
    string? ShippingReference
);
