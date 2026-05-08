namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Harici bir servis (AI, dosya depolama, mesajlaşma vb.) çağrısı başarısız olduğunda fırlatılır.
/// HTTP 502 Bad Gateway olarak işlenir.
/// </summary>
public class ExternalServiceException : Exception
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}
