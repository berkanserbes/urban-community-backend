namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// JWT access token veya refresh token geçersiz, manipüle edilmiş veya süresi dolmuş olduğunda fırlatılır.
/// HTTP 401 Unauthorized olarak işlenir.
/// </summary>
public class InvalidTokenException : Exception
{
    public InvalidTokenException(string message = "Token geçersiz veya süresi dolmuş.")
        : base(message)
    {
    }
}
