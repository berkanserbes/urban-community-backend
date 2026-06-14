using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReportById;

public sealed record GetReportByIdQuery(Guid Id) : IRequest<Result<GetReportByIdQueryResponse>>;
