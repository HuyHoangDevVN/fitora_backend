using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetGroupById;

public class GetGroupByIdHandler (IGroupRepository groupRepo, IMapper mapper) : IQueryHandler<GetGroupByIdQuery, GroupDto>
{
    public  async Task<GroupDto> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await groupRepo.GetGroupByIdAsync(request.Id);
        return result!;
    }
}