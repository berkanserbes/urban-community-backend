namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Kullanıcı belirli bir sürede izin verilen istek limitini aştığında fırlatılır.
/// HTTP 429 Too Many Requests olarak işlenir.
/// </summary>
public class RateLimitExceededException : Exception
{
    public int? RetryAfterSeconds { get; }

    public RateLimitExceededException(int? retryAfterSeconds = null)
        : base(retryAfterSeconds.HasValue
            ? $"Çok fazla istek gönderildi. {retryAfterSeconds} saniye sonra tekrar deneyin."
            : "Çok fazla istek gönderildi. Lütfen bir süre bekleyip tekrar deneyin.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
