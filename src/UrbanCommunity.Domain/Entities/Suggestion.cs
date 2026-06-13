using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Suggestion : BaseEntity
{
    private static readonly IReadOnlyDictionary<SuggestionStatus, SuggestionStatus[]> AllowedStatusTransitions =
        new Dictionary<SuggestionStatus, SuggestionStatus[]>
        {
            [SuggestionStatus.Active] = [SuggestionStatus.Reviewing],
            [SuggestionStatus.Reviewing] =
            [
                SuggestionStatus.Active,
                SuggestionStatus.Implementation,
                SuggestionStatus.Closed
            ],
            [SuggestionStatus.Implementation] =
            [
                SuggestionStatus.Reviewing,
                SuggestionStatus.Closed
            ],
            [SuggestionStatus.Closed] = []
        };

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public SuggestionStatus Status { get; set; } = SuggestionStatus.Active;
    public int VoteScore { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<SuggestionComment> Comments { get; set; } = new List<SuggestionComment>();

    public IReadOnlyCollection<SuggestionStatus> GetAllowedNextStatuses()
    {
        return AllowedStatusTransitions.TryGetValue(Status, out var allowedStatuses)
            ? allowedStatuses
            : Array.Empty<SuggestionStatus>();
    }

    public bool CanTransitionTo(SuggestionStatus newStatus)
    {
        return newStatus == Status || GetAllowedNextStatuses().Contains(newStatus);
    }

    public void ChangeStatus(SuggestionStatus newStatus)
    {
        if (newStatus == Status)
            return;

        if (!CanTransitionTo(newStatus))
        {
            throw new DomainException(
                $"Geçersiz öneri durum geçişi: {Status} -> {newStatus}");
        }

        Status = newStatus;
    }

    public bool AllowsVotes() => Status == SuggestionStatus.Active;

    public bool AllowsCommentChanges() => Status != SuggestionStatus.Closed;
}
