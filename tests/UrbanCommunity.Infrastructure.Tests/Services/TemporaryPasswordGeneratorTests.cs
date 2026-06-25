using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class TemporaryPasswordGeneratorTests
{
    [Fact]
    public void Generate_CreatesLongComplexPasswords()
    {
        var generator = new TemporaryPasswordGenerator();

        var passwords = Enumerable.Range(0, 50)
            .Select(_ => generator.Generate())
            .ToList();

        Assert.All(passwords, password =>
        {
            Assert.True(password.Length >= 16);
            Assert.Contains(password, char.IsUpper);
            Assert.Contains(password, char.IsLower);
            Assert.Contains(password, char.IsDigit);
            Assert.Contains(password, character => !char.IsLetterOrDigit(character));
        });
        Assert.Equal(passwords.Count, passwords.Distinct().Count());
    }
}
