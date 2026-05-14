namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Yüklenen dosyanın türü (MIME type / uzantı) kabul edilmediğinde fırlatılır.
/// HTTP 415 Unsupported Media Type olarak işlenir.
/// </summary>
public class InvalidFileTypeException : Exception
{
    public string? AttemptedContentType { get; }
    public IReadOnlyList<string> AllowedTypes { get; }

    public InvalidFileTypeException(string? attemptedContentType, IReadOnlyList<string> allowedTypes)
        : base($"Geçersiz dosya türü: '{attemptedContentType}'. İzin verilen türler: {string.Join(", ", allowedTypes)}")
    {
        AttemptedContentType = attemptedContentType;
        AllowedTypes = allowedTypes;
    }

    public InvalidFileTypeException(string message) : base(message)
    {
        AllowedTypes = [];
    }
}
