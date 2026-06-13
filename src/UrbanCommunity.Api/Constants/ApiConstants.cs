namespace UrbanCommunity.Api.Constants;

/// <summary>
/// API layer constants used across the application.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// Maximum upload size limit: 10 GB.
    /// </summary>
    public const long MaxUploadSize = 10_737_418_240;

    /// <summary>
    /// Reports support up to ten media files, each limited to 50 MB.
    /// </summary>
    public const long MaxReportUploadSize = 524_288_000;
}
