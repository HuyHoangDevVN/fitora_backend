using BuildingBlocks.DTOs;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.UpdateGroup;

public class UpdateGroupHandler(
    IGroupRepository groupRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth,
    IMapper mapper) : ICommandHandler<UpdateGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == command.Request.Id && m.UserId == callerId, cancellationToken);
        if (membership == null)
            return new ResponseDto(null, false, "Bạn không phải thành viên của nhóm");

        if (membership.Role != GroupRole.Owner && membership.Role != GroupRole.Admin)
            return new ResponseDto(null, false, "Chỉ Owner/Admin mới được cập nhật thông tin nhóm");

        var group = mapper.Map<Domain.Models.Group>(command.Request);
        var result = await groupRepo.UpdateAsync(group);
        return new ResponseDto(
            null, result, result ? "Cập nhật nhóm thành công!" : "Cập nhật nhóm thất bại!"
        );
    }
}