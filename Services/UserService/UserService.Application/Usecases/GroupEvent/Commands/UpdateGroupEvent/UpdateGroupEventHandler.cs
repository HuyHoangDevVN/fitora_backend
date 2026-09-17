using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupEvent.Commands.UpdateGroupEvent;

public class UpdateGroupEventHandler(
    IRepositoryBase<Domain.Models.GroupEvent> eventRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<UpdateGroupEventCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateGroupEventCommand request, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var ev = await eventRepo.GetAsync(e => e.Id == request.EventId, ct)
            ?? throw new NotFoundException("Sự kiện không tồn tại");

        var membership = await memberRepo.GetAsync(
            m => m.GroupId == ev.GroupId && m.UserId == callerId, ct);
        if (membership is null)
            throw new UnAuthorizationException("Chỉ thành viên mới chỉnh sửa sự kiện.");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được chỉnh sửa sự kiện.");

        ev.Title = request.Title;
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.Location = request.Location;

        await eventRepo.UpdateAsync(e => e.Id == ev.Id, ev, ct);
        var ok = await eventRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Cập nhật sự kiện thành công" : "Cập nhật sự kiện thất bại");
    }
}
