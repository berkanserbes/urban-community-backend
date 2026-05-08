namespace UrbanCommunity.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key) 
        : base($"{name} ({key}) bulunamadı.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}
