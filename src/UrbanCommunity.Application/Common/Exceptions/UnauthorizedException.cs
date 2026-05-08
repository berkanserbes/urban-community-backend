namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Kimlik doğrulaması gerektiren bir işleme yetkisiz erişim yapıldığında fırlatılır.
/// HTTP 401 Unauthorized olarak işlenir.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Bu işlem için kimlik doğrulaması gereklidir.")
        : base(message)
    {
    }
}
