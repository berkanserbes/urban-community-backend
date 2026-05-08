namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Hesabı deaktive edilmiş (silinmiş/yasaklanmış) kullanıcı işlem yapmaya çalıştığında fırlatılır.
/// HTTP 403 Forbidden olarak işlenir.
/// </summary>
public class AccountDeactivatedException : Exception
{
    public AccountDeactivatedException(string message = "Hesabınız deaktive edilmiştir. Lütfen destek ile iletişime geçin.")
        : base(message)
    {
    }
}
