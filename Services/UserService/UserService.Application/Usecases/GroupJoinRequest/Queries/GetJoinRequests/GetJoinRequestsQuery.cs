using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.GroupJoinRequest.Queries.GetJoinRequests;

public record GetJoinRequestsQuery(Guid GroupId, int PageIndex = 0, int PageSize = 20)
    : IQuery<PaginatedResult<Domain.Models.GroupJoinRequest>>;
