using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Queries.GetBlockedGroups;

public class GetBlockedGroupsHandler(IBlockRepository blockRepository)
    : IQueryHandler<GetBlockedGroupsQuery, PaginatedResult<BlockedGroup>>
{
    public Task<PaginatedResult<BlockedGroup>> Handle(GetBlockedGroupsQuery request, CancellationToken cancellationToken)
        => blockRepository.GetBlockedGroupsAsync(request.BlockerId, request.PageIndex, request.PageSize);
}
