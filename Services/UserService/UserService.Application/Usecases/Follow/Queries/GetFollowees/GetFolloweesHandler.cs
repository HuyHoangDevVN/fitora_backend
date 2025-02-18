using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Follow.Queries.GetFollowees;

public class GetFolloweesHandler (IFollowRepository followRepo, IMapper mapper) : IQueryHandler<GetFolloweesQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetFolloweesQuery request, CancellationToken cancellationToken)
    {
        var result = await followRepo.GetFollowersAsync(request.Request, true);
        return new ResponseDto(result);
    }
}