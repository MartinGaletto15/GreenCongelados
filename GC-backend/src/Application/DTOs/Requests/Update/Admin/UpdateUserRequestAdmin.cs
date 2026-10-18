using Domain.Entities.Enums;

namespace Application.DTOs.Requests;

public record UpdateUserRequestAdmin (
    Role? role
); 