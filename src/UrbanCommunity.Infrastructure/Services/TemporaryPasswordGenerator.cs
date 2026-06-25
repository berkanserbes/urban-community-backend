using System.Security.Cryptography;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class TemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@$%*-_";
    private const int Length = 18;

    public string Generate()
    {
        var all = Uppercase + Lowercase + Digits + Symbols;
        var characters = new List<char>
        {
            Pick(Uppercase),
            Pick(Lowercase),
            Pick(Digits),
            Pick(Symbols)
        };

        while (characters.Count < Length)
            characters.Add(Pick(all));

        for (var index = characters.Count - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (characters[index], characters[swapIndex]) = (characters[swapIndex], characters[index]);
        }

        return new string(characters.ToArray());
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
