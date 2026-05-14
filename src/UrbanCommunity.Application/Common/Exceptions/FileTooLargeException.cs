namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Yüklenen dosyanın boyutu izin verilen maksimum sınırı aştığında fırlatılır.
/// HTTP 413 Request Entity Too Large olarak işlenir.
/// </summary>
public class FileTooLargeException : Exception
{
    public long FileSizeBytes { get; }
    public long MaxAllowedBytes { get; }

    public FileTooLargeException(long fileSizeBytes, long maxAllowedBytes)
        : base($"Dosya boyutu ({fileSizeBytes / 1024 / 1024} MB) maksimum izin verilen boyutu ({maxAllowedBytes / 1024 / 1024} MB) aşıyor.")
    {
        FileSizeBytes = fileSizeBytes;
        MaxAllowedBytes = maxAllowedBytes;
    }

    public FileTooLargeException(string message) : base(message)
    {
    }
}
