namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentById;

public sealed record GetDepartmentByIdQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
