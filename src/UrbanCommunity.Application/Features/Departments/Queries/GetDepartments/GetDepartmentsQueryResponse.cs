namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartments;

public sealed record GetDepartmentsQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone);
