using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class District : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid ProvinceId { get; set; }
    public Province Province { get; set; } = null!;

    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
}
