using System.ComponentModel.DataAnnotations;

namespace Applications.dtos.Requests;

public record UpdateAddressRequest(
    [MaxLength(100)] string? Street,
    [MaxLength(10)] string? Dpto,
    string? References
);
