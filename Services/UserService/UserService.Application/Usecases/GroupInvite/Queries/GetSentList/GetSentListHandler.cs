using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetSentList;

public class GetSentListHandler(IGroupInviteRepository groupInviteRepo) : IQueryHandler<GetSentListQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetSentListQuery request, CancellationToken cancellationToken)
    {
        var result = await groupInviteRepo.GetSentListAsync(request.Request);
        return (result);
    }
}