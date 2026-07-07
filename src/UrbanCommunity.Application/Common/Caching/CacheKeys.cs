namespace UrbanCommunity.Application.Common.Caching;

public static class CacheKeys
{
    private const string SchemaVersion = "v1";

    public const string ProvincesList = $"{SchemaVersion}:ref:provinces:list";
    public const string CategoriesList = $"{SchemaVersion}:ref:categories:list";
    public const string DepartmentsList = $"{SchemaVersion}:ref:departments:list";
    public const string NeighborhoodsListPrefix = $"{SchemaVersion}:ref:neighborhoods:list";

    public static string DistrictsByProvince(Guid provinceId)
    {
        return $"{SchemaVersion}:ref:districts:list:province-id:{FormatId(provinceId)}";
    }

    public static string NeighborhoodsList(Guid? provinceId, Guid? districtId)
    {
        return $"{SchemaVersion}:ref:neighborhoods:list:province-id:{FormatIdOrAll(provinceId)}:district-id:{FormatIdOrAll(districtId)}";
    }

    private static string FormatId(Guid id)
    {
        return id.ToString("D");
    }

    private static string FormatIdOrAll(Guid? id)
    {
        return id.HasValue ? FormatId(id.Value) : "all";
    }
}
