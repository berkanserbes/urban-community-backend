using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class ReportCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<OrganizationDepartmentCategory> OrganizationDepartments { get; set; } =
        new List<OrganizationDepartmentCategory>();
}
