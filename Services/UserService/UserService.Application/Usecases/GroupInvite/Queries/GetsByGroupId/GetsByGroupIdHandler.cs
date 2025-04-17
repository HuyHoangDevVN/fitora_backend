using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetsByGroupId;

public class GetsByGroupIdHandler (IGroupInviteRepository groupInviteRepo) : IQueryHandler<GetsByGroupIdQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetsByGroupIdQuery request, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.GetsByGroupIdAsync(request.Request);
        return (result);
    }
}