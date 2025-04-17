using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetReceivedList;

public class GetReceivedListHandler (IGroupInviteRepository groupInviteRepo) : IQueryHandler<GetReceivedListQuery,ResponseDto>
{
    public async Task<ResponseDto> Handle(GetReceivedListQuery request, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.GetReceivedListAsync(request.Request);
        return (result);
    }
}