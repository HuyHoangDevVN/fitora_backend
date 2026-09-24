using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupJoinRequest.Queries.GetJoinRequests;

public class GetJoinRequestsHandler(
    IRepositoryBase<Domain.Models.GroupJoinRequest> requestRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : IQueryHandler<GetJoinRequestsQuery, PaginatedResult<Domain.Models.GroupJoinRequest>>
{
    public async Task<PaginatedResult<Domain.Models.GroupJoinRequest>> Handle(
        GetJoinRequestsQuery req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == callerId, ct);
        if (membership is null || membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được xem yêu cầu tham gia");

        return await requestRepo.GetPageAsync(
            new PaginationRequest(req.PageIndex, req.PageSize), ct,
            r => r.GroupId == req.GroupId && r.Status == GroupJoinRequestStatus.Pending);
    }
}
