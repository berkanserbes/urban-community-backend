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

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

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

    public void ChangeRole(UserRole role)
    {
        Role = role;

        if (role != UserRole.Official)
        {
            DepartmentId = null;
            Department = null;
        }
    }

    public void AssignDepartment(Guid? departmentId)
    {
        if (departmentId.HasValue && Role != UserRole.Official)
        {
            throw new DomainException("Departman ataması yalnızca Official rolündeki kullanıcılara yapılabilir.");
        }

        DepartmentId = departmentId;

        if (!departmentId.HasValue)
        {
            Department = null;
        }
    }

    public void UpdateProfile(string firstName, string lastName, string? bio, string? profileImageUrl)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
        ProfileImageUrl = string.IsNullOrWhiteSpace(profileImageUrl) ? null : profileImageUrl.Trim();
    }

    public void SetProfileImage(string? imageUrl)
    {
        ProfileImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
