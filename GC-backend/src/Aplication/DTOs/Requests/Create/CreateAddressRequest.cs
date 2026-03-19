using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record CreateAddressRequest(
    [Required] [MaxLength(100)] string Street,
    [MaxLength(10)] string? Dpto,
    string? References
);