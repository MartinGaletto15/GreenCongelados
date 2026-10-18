using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Requests;

public record UpdateAddressRequest(
    [MaxLength(100)] string? Street,
    [MaxLength(10)] string? Dpto,
    string? References
);
