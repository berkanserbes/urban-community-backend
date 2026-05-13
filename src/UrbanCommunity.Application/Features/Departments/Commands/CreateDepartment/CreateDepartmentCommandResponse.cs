namespace UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;

public sealed record CreateDepartmentCommandResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
