namespace UrbanCommunity.Application.Common.Exceptions;

/// <summary>
/// Eş zamanlı güncelleme çakışması (optimistic concurrency) oluştuğunda fırlatılır.
/// Başka bir istek aynı kaynağı önceden değiştirmiş demektir.
/// HTTP 409 Conflict olarak işlenir.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message = "Kayıt başka bir işlem tarafından değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin.")
        : base(message)
    {
    }

    public ConcurrencyException(string entityName, object key)
        : base($"{entityName} ({key}) kaydı güncellenirken çakışma oluştu.")
    {
    }
}
