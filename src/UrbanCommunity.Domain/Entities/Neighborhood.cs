using NetTopologySuite.Geometries;
using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Neighborhood : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    // Koordinat veya Sınır verisi (PostGIS Geo-Spatial alan)
    public Polygon? PolygonData { get; set; }

    // İlişkiler
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<CommunityPost> CommunityPosts { get; set; } = new List<CommunityPost>();
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}
