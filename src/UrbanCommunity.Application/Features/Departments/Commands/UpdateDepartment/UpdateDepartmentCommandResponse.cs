namespace UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;

public sealed record UpdateDepartmentCommandResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
