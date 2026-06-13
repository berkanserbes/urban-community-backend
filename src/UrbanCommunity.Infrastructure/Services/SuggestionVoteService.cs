using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Infrastructure.Persistence;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class SuggestionVoteService(ApplicationDbContext context)
    : ISuggestionVoteService
{
    public async Task<SuggestionVoteMutationResult> SetVoteAsync(
        Guid suggestionId,
        Guid userId,
        bool isUpvote,
        CancellationToken cancellationToken)
    {
        await EnsureActiveUserExistsAsync(userId, cancellationToken);
        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken);

        var suggestion = await GetLockedSuggestionAsync(
            suggestionId,
            cancellationToken);

        if (!suggestion.AllowsVotes())
        {
            return await RollbackFailureAsync(
                transaction,
                "Yalnızca aktif önerilere oy verilebilir.",
                suggestion.VoteScore,
                cancellationToken);
        }

        var existingVote = await context.Votes.FirstOrDefaultAsync(
            vote => vote.SuggestionId == suggestionId && vote.UserId == userId,
            cancellationToken);

        var newValue = isUpvote ? 1 : -1;
        var scoreDelta = newValue;

        if (existingVote is null)
        {
            context.Votes.Add(new Vote
            {
                SuggestionId = suggestionId,
                UserId = userId,
                IsUpvote = isUpvote
            });
        }
        else
        {
            var oldValue = existingVote.IsUpvote ? 1 : -1;
            scoreDelta = newValue - oldValue;
            existingVote.IsUpvote = isUpvote;
        }

        suggestion.VoteScore += scoreDelta;
        await context.SaveChangesAsync(cancellationToken);

        var counts = await GetVoteCountsAsync(suggestionId, cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return new SuggestionVoteMutationResult(
            true,
            null,
            counts.UpvoteCount,
            counts.DownvoteCount,
            suggestion.VoteScore,
            isUpvote);
    }

    public async Task<SuggestionVoteMutationResult> RemoveVoteAsync(
        Guid suggestionId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await EnsureActiveUserExistsAsync(userId, cancellationToken);
        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken);

        var suggestion = await GetLockedSuggestionAsync(
            suggestionId,
            cancellationToken);

        if (!suggestion.AllowsVotes())
        {
            return await RollbackFailureAsync(
                transaction,
                "Yalnızca aktif önerilerdeki oylar kaldırılabilir.",
                suggestion.VoteScore,
                cancellationToken);
        }

        var vote = await context.Votes.FirstOrDefaultAsync(
            item => item.SuggestionId == suggestionId && item.UserId == userId,
            cancellationToken);

        if (vote is null)
        {
            return await RollbackFailureAsync(
                transaction,
                "Bu öneri için oyunuz bulunmuyor.",
                suggestion.VoteScore,
                cancellationToken);
        }

        suggestion.VoteScore += vote.IsUpvote ? -1 : 1;
        context.Votes.Remove(vote);
        await context.SaveChangesAsync(cancellationToken);

        var counts = await GetVoteCountsAsync(suggestionId, cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return new SuggestionVoteMutationResult(
            true,
            null,
            counts.UpvoteCount,
            counts.DownvoteCount,
            suggestion.VoteScore,
            null);
    }

    private async Task EnsureActiveUserExistsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId && user.IsActive,
                cancellationToken);

        if (!exists)
            throw new NotFoundException(nameof(User), userId);
    }

    private async Task<Suggestion> GetLockedSuggestionAsync(
        Guid suggestionId,
        CancellationToken cancellationToken)
    {
        Suggestion? suggestion;

        if (context.Database.IsNpgsql())
        {
            suggestion = await context.Suggestions
                .FromSqlInterpolated(
                    $"""
                     SELECT *
                     FROM "Suggestions"
                     WHERE "Id" = {suggestionId}
                     FOR UPDATE
                     """)
                .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            suggestion = await context.Suggestions.FirstOrDefaultAsync(
                item => item.Id == suggestionId,
                cancellationToken);
        }

        return suggestion
            ?? throw new NotFoundException(nameof(Suggestion), suggestionId);
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync(
        CancellationToken cancellationToken)
    {
        return context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync(cancellationToken)
            : null;
    }

    private async Task<SuggestionVoteMutationResult> RollbackFailureAsync(
        IDbContextTransaction? transaction,
        string errorMessage,
        int voteScore,
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
            await transaction.RollbackAsync(cancellationToken);

        return new SuggestionVoteMutationResult(
            false,
            errorMessage,
            0,
            0,
            voteScore,
            null);
    }

    private async Task<VoteCounts> GetVoteCountsAsync(
        Guid suggestionId,
        CancellationToken cancellationToken)
    {
        return await context.Votes
            .AsNoTracking()
            .Where(vote => vote.SuggestionId == suggestionId)
            .GroupBy(_ => 1)
            .Select(group => new VoteCounts(
                group.Count(vote => vote.IsUpvote),
                group.Count(vote => !vote.IsUpvote)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? new VoteCounts(0, 0);
    }

    private sealed record VoteCounts(int UpvoteCount, int DownvoteCount);
}
