using System.ComponentModel.DataAnnotations;
using Domain.Enums.Entities;

namespace Applications.dtos.Requests;

public record UpdateAddressRequest(
    [MaxLength(100)] string? Street,
    Cities? City,
    [MaxLength(8)] string? ZipCode,
    [MaxLength(10)] string? Dpto,
    string? References
);
