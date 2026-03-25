using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Applications.dtos.Requests;

public record CreateAddressRequest(
    [Required] [MaxLength(100)] string Street,
    [MaxLength(10)] string? Dpto,
    string? References
);