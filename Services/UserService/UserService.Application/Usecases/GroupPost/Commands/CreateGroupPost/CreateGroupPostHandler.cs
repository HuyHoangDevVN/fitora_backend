using BuildingBlocks.DTOs;
using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Usecases.GroupPost.Commands.CreateGroupPost;

public class CreateGroupPostHandler(
    IGroupPostRepository groupPostRepo,
    IRepositoryBase<Domain.Models.Group> groupRepo,
    IMapper mapper) : ICommandHandler<CreateGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupPostCommand command, CancellationToken ct)
    {
        var groupPost = mapper.Map<Domain.Models.GroupPost>(command.Request);

        // If group requires approval, new post starts as Pending
        var group = await groupRepo.GetAsync(g => g.Id == command.Request.GroupId, ct);
        if (group is not null && group.RequirePostApproval)
        {
            groupPost.ApprovalStatus = Domain.Enums.ApprovalStatus.Pending;
            groupPost.IsApproved = false;
        }
        else
        {
            groupPost.ApprovalStatus = Domain.Enums.ApprovalStatus.Approved;
            groupPost.IsApproved = true;
            groupPost.ApprovedAt = DateTime.UtcNow;
        }

        var result = await groupPostRepo.CreateAsync(groupPost);
        return new ResponseDto(result);
    }
}