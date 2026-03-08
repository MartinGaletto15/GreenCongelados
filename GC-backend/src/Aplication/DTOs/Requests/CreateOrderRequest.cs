using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record CreateOrderRequest(
    [Required] int IdUser,
    int? IdCourier,
    int? IdPromotion,
    [Required] decimal ShippingCost,
    [Required] string ShippingAddress,
    [Required] DateTime OrderDate,
    decimal GlobalDiscount,
    [Required] decimal Subtotal,
    [Required] decimal Total,
    [Required] OrderStatus OrderStatus,
    List<CreateOrderItemRequest> OrderItems
);
