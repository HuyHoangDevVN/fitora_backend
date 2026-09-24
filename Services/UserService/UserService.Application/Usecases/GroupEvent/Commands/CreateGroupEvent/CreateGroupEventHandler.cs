using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupEvent.Commands.CreateGroupEvent;

public class CreateGroupEventHandler(
    IRepositoryBase<Domain.Models.GroupEvent> eventRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<CreateGroupEventCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupEventCommand request, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == request.GroupId && m.UserId == callerId, ct);
        if (membership is null) throw new UnAuthorizationException("Chỉ thành viên mới tạo sự kiện.");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được tổ chức sự kiện.");

        var ev = new Domain.Models.GroupEvent
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            Title = request.Title,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
        };
        await eventRepo.AddAsync(ev, ct);
        var ok = await eventRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Tạo sự kiện thành công" : "Tạo sự kiện thất bại");
    }
}
