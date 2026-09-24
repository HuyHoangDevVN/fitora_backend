using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Usecases.GroupPost.Queries.GetPendingGroupPosts;

public class GetPendingGroupPostsHandler(IRepositoryBase<Domain.Models.GroupPost> postRepo)
    : IQueryHandler<GetPendingGroupPostsQuery, PaginatedResult<Domain.Models.GroupPost>>
{
    public async Task<PaginatedResult<Domain.Models.GroupPost>> Handle(
        GetPendingGroupPostsQuery req, CancellationToken ct)
    {
        return await postRepo.GetPageAsync(
            new PaginationRequest(req.PageIndex, req.PageSize), ct,
            p => p.GroupId == req.GroupId && p.ApprovalStatus == Domain.Enums.ApprovalStatus.Pending);
    }
}
