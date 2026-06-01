namespace UrbanCommunity.Application.Common.Exceptions;

public class InvalidInputException : Exception
{
    public InvalidInputException(string message) : base(message)
    {
    }
}