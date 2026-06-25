using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => string.Join(' ', new[] { FirstName, LastName }.Where(name => !string.IsNullOrWhiteSpace(name)));

    public UserRole Role { get; set; } = UserRole.Citizen;

    public Guid? NeighborhoodId { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    public Guid? OrganizationDepartmentId { get; set; }
    public OrganizationDepartment? OrganizationDepartment { get; set; }

    public OfficialJurisdictionLevel? OfficialJurisdictionLevel { get; set; }

    public Guid? OfficialProvinceId { get; set; }
    public Province? OfficialProvince { get; set; }

    public Guid? OfficialDistrictId { get; set; }
    public District? OfficialDistrict { get; set; }

    public Guid? OfficialNeighborhoodId { get; set; }
    public Neighborhood? OfficialNeighborhood { get; set; }

    // Profil Bilgileri
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }

    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<Suggestion> Suggestions { get; set; } = new List<Suggestion>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public ICollection<ReportComment> ReportComments { get; set; } = new List<ReportComment>();
    public ICollection<SuggestionComment> SuggestionComments { get; set; } = new List<SuggestionComment>();
    public ICollection<CommunityPostComment> CommunityPostComments { get; set; } = new List<CommunityPostComment>();
    public ICollection<ReportHistory> ChangedReportHistories { get; set; } = new List<ReportHistory>();

    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public void SetUpdatedAt(DateTime now) => UpdatedAt = now;

    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; private set; }

    public void UpdateAccess(
        UserRole role,
        Guid? organizationDepartmentId = null,
        OfficialJurisdictionLevel? jurisdictionLevel = null,
        Guid? jurisdictionId = null)
    {
        Role = role;

        if (role != UserRole.Official)
        {
            ClearOfficialAccess();
            return;
        }

        if (!organizationDepartmentId.HasValue || !jurisdictionLevel.HasValue || !jurisdictionId.HasValue)
            throw new DomainException("Official rolü için departman ve yetki alanı zorunludur.");

        OrganizationDepartmentId = organizationDepartmentId;
        OrganizationDepartment = null;
        OfficialJurisdictionLevel = jurisdictionLevel;
        OfficialProvinceId = null;
        OfficialProvince = null;
        OfficialDistrictId = null;
        OfficialDistrict = null;
        OfficialNeighborhoodId = null;
        OfficialNeighborhood = null;

        switch (jurisdictionLevel.Value)
        {
            case UrbanCommunity.Domain.Enums.OfficialJurisdictionLevel.Province:
                OfficialProvinceId = jurisdictionId;
                break;
            case UrbanCommunity.Domain.Enums.OfficialJurisdictionLevel.District:
                OfficialDistrictId = jurisdictionId;
                break;
            case UrbanCommunity.Domain.Enums.OfficialJurisdictionLevel.Neighborhood:
                OfficialNeighborhoodId = jurisdictionId;
                break;
            default:
                throw new DomainException("Geçersiz Official yetki alanı seviyesi.");
        }
    }

    public void UpdateProfile(string firstName, string lastName, string? bio)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
    }

    public void SetProfileImage(string? imageUrl)
    {
        ProfileImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    public void AssignNeighborhood(Guid? neighborhoodId)
    {
        NeighborhoodId = neighborhoodId;
    }

    private void ClearOfficialAccess()
    {
        OrganizationDepartmentId = null;
        OrganizationDepartment = null;
        OfficialJurisdictionLevel = null;
        OfficialProvinceId = null;
        OfficialProvince = null;
        OfficialDistrictId = null;
        OfficialDistrict = null;
        OfficialNeighborhoodId = null;
        OfficialNeighborhood = null;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void RequirePasswordChange() => MustChangePassword = true;

    public void CompleteRequiredPasswordChange() => MustChangePassword = false;

    public string GetUserFullName() => string.Join(' ', new[] { FirstName, LastName }.Where(name => !string.IsNullOrWhiteSpace(name)));
}
