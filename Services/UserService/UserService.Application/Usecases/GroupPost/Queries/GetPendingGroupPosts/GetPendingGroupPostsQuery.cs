using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.GroupPost.Queries.GetPendingGroupPosts;

public record GetPendingGroupPostsQuery(Guid GroupId, int PageIndex = 0, int PageSize = 20)
    : IQuery<PaginatedResult<Domain.Models.GroupPost>>;
