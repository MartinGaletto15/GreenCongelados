using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record CreateAddressRequest(
    [Required] [MaxLength(100)] string Street,
    [Required] Cities City,
    [Required] [MaxLength(10)] string ZipCode,
    [MaxLength(10)] string? Dpto,
    string? References
);
