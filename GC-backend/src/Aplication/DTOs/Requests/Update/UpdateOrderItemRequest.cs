using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateOrderItemRequest(
    int? Quantity
);