namespace UrbanCommunity.Application.Interfaces;

public interface IOrganizationDepartmentAssignmentService
{
    Task<OrganizationDepartmentAssignment?> ResolveAsync(
        Guid categoryId,
        Guid neighborhoodId,
        CancellationToken cancellationToken);

    Task<OrganizationDepartmentAssignment?> GetCompatibleAsync(
        Guid organizationDepartmentId,
        Guid neighborhoodId,
        CancellationToken cancellationToken);

    Task<bool> IsCompatibleAsync(
        Guid organizationDepartmentId,
        Guid categoryId,
        Guid neighborhoodId,
        CancellationToken cancellationToken);
}

public sealed record OrganizationDepartmentAssignment(
    Guid OrganizationDepartmentId,
    Guid OrganizationId,
    string OrganizationName,
    Guid DepartmentId,
    string DepartmentName,
    string? ContactEmail,
    string? ContactPhone);
