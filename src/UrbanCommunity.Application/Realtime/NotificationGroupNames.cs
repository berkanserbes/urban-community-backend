namespace UrbanCommunity.Application.Realtime;

public static class NotificationGroupNames
{
    public const string All = "all";

    public static string Province(Guid provinceId) =>
        $"province_{provinceId:D}";

    public static string District(Guid districtId) =>
        $"district_{districtId:D}";

    public static string Neighborhood(Guid neighborhoodId) =>
        $"neighborhood_{neighborhoodId:D}";
}
