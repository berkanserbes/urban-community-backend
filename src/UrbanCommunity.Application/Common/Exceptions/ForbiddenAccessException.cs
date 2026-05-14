namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Kimliği doğrulanmış kullanıcı bu kaynağa erişim yetkisine sahip olmadığında fırlatılır.
/// HTTP 403 Forbidden olarak işlenir.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır.")
        : base(message)
    {
    }
}
