using Microsoft.EntityFrameworkCore;
using Npgsql;
using UrbanCommunity.Infrastructure.Consumers;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Messaging;

public sealed class DatabaseConflictRetryTests
{
    [Theory]
    [InlineData(PostgresErrorCodes.SerializationFailure)]
    [InlineData(PostgresErrorCodes.DeadlockDetected)]
    [InlineData(PostgresErrorCodes.UniqueViolation)]
    public void ConflictIsRetriedWhenWrappedByEfCore(string sqlState)
    {
        var postgres = new PostgresException("conflict", "ERROR", "ERROR", sqlState);

        // The inbox lock query surfaces a serialization failure wrapped like this.
        Assert.True(ReportAiReviewRequestedConsumerDefinition.IsDatabaseConflict(
            new InvalidOperationException("transient failure", postgres)));
        Assert.True(ReportAiReviewRequestedConsumerDefinition.IsDatabaseConflict(
            new DbUpdateException("save failed", postgres)));
        Assert.True(ReportAiReviewRequestedConsumerDefinition.IsDatabaseConflict(postgres));
    }

    [Fact]
    public void OtherFailuresAreNotRetried()
    {
        var foreignKey = new PostgresException("fk", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation);

        Assert.False(ReportAiReviewRequestedConsumerDefinition.IsDatabaseConflict(
            new DbUpdateException("save failed", foreignKey)));
        Assert.False(ReportAiReviewRequestedConsumerDefinition.IsDatabaseConflict(
            new InvalidOperationException("bug")));
    }
}
