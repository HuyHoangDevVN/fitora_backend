using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;
using UserService.Application.Services.IServices;

namespace UserService.Application.Usecases.Blocks.Queries.GetBlockedUsers;

public class GetBlockedUsersHandler(IBlockRepository blockRepository)
    : IQueryHandler<GetBlockedUsersQuery, PaginatedResult<Block>>
{
    public Task<PaginatedResult<Block>> Handle(GetBlockedUsersQuery request, CancellationToken cancellationToken)
        => blockRepository.GetBlockedUsersAsync(request.BlockerId, request.PageIndex, request.PageSize);
}
