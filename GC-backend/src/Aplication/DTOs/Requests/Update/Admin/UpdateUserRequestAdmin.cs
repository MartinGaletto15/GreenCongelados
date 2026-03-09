using Domain.Entities.Enums;

namespace Applications.dtos.Requests;

public record UpdateUserRequestAdmin (
    Role? role
); 