using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record CreateShippingCostRequest(
    [Required] [MaxLength(50)] string Name,
    [Required] decimal Cost
);
