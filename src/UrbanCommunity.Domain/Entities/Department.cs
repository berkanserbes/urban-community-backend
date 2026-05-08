using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Örn: Su İşleri Müdürlüğü
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public ICollection<User> Officials { get; set; } = new List<User>();

    // Bu birime atanmış raporlar
    public ICollection<Report> AssignedReports { get; set; } = new List<Report>();

    public void UpdateDetails(string name, string? description, string? contactEmail, string? contactPhone)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
    }
}
