using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record CreateAddressRequest(
    [Required] [MaxLength(100)] string Street,
    [MaxLength(10)] string? Dpto,
    string? References
);