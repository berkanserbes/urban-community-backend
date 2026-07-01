using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public OrganizationJurisdictionLevel JurisdictionLevel { get; set; }

    public Guid ProvinceId { get; set; }
    public Province Province { get; set; } = null!;

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public Guid? NeighborhoodId { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    public bool IsActive { get; private set; } = true;
    public ICollection<OrganizationDepartment> Departments { get; set; } = new List<OrganizationDepartment>();

    public void Update(
        string name,
        OrganizationJurisdictionLevel jurisdictionLevel,
        Guid provinceId,
        Guid? districtId,
        Guid? neighborhoodId)
    {
        Name = name.Trim();
        JurisdictionLevel = jurisdictionLevel;
        ProvinceId = provinceId;
        DistrictId = districtId;
        NeighborhoodId = neighborhoodId;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
