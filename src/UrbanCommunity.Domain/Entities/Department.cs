using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<OrganizationDepartment> OrganizationDepartments { get; set; } =
        new List<OrganizationDepartment>();

    public void UpdateDetails(string name, string? description)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
