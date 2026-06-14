using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UrbanCommunity.Application.Features.Reports;

public static partial class ReportSimilarityFingerprint
{
    public static string Create(string title, string description)
    {
        var normalizedContent = Normalize($"{title} {description}");
        return Convert.ToHexStringLower(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalizedContent)));
    }

    private static string Normalize(string value) =>
        WhitespaceRegex().Replace(value.Trim().ToLowerInvariant(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
