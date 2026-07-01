using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class OrganizationDepartment : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public string? DisplayName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<OrganizationDepartmentCategory> Categories { get; set; } =
        new List<OrganizationDepartmentCategory>();
    public ICollection<User> Officials { get; set; } = new List<User>();
    public ICollection<Report> AssignedReports { get; set; } = new List<Report>();

    public void UpdateDetails(string? displayName, string? contactEmail, string? contactPhone)
    {
        DisplayName = Normalize(displayName);
        ContactEmail = Normalize(contactEmail);
        ContactPhone = Normalize(contactPhone);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
