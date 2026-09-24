using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.DeleteGroup;

/// <summary>
/// Chỉ Owner mới được xóa nhóm (cùng mức quyền như Dissolve — về bản chất là
/// hành động tương đương). Trước đây handler này gọi thẳng repo không check gì.
/// </summary>
public class DeleteGroupHandler(
    IGroupRepository groupRepo,
    IMapper mapper,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension authorize) : ICommandHandler<DeleteGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
    {
        var callerId = authorize.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == command.Id && m.UserId == callerId, cancellationToken);
        if (membership is null || membership.Role != GroupRole.Owner)
            throw new UnAuthorizationException("Chỉ Owner mới được xóa nhóm.");

        var result = await groupRepo.DeleteAsync(command.Id);
        return new ResponseDto(
            null, result, result ? "Xóa nhóm thành công." : "Xóa nhóm thất bại."
        );
    }
}