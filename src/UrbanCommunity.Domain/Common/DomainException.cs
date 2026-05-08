namespace UrbanCommunity.Domain.Common;

/// <summary>
/// Domain iş kuralı ihlali durumunda fırlatılır.
/// HTTP 422 Unprocessable Entity olarak işlenir.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
