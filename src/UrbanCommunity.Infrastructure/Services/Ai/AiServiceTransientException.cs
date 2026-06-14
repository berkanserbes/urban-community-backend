namespace UrbanCommunity.Infrastructure.Services.Ai;

internal sealed class AiServiceTransientException : Exception
{
    public AiServiceTransientException(string message)
        : base(message)
    {
    }

    public AiServiceTransientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
