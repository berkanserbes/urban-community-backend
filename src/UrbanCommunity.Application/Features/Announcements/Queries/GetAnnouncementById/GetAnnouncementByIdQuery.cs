using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncementById;

public sealed record GetAnnouncementByIdQuery(Guid Id) : IRequest<Result<GetAnnouncementByIdQueryResponse>>;
