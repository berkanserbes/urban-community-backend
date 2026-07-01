namespace UrbanCommunity.Domain.Entities;

public class OrganizationDepartmentCategory
{
    public Guid OrganizationDepartmentId { get; set; }
    public OrganizationDepartment OrganizationDepartment { get; set; } = null!;

    public Guid ReportCategoryId { get; set; }
    public ReportCategory ReportCategory { get; set; } = null!;
}
