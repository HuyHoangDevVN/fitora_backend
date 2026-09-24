using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;

namespace UserService.Application.Usecases.GroupEvent.Commands.RsvpEvent;

public class RsvpEventHandler(
    IRepositoryBase<Domain.Models.GroupEvent> eventRepo,
    IRepositoryBase<Domain.Models.EventRsvp> rsvpRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<RsvpEventCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(RsvpEventCommand request, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var ev = await eventRepo.GetAsync(e => e.Id == request.EventId, ct)
            ?? throw new NotFoundException("Sự kiện không tồn tại");
        // Cần là thành viên nhóm
        var isMember = await memberRepo.GetAsync(
            m => m.GroupId == ev.GroupId && m.UserId == callerId, ct) is not null;
        if (!isMember) throw new UnAuthorizationException("Bạn chưa là thành viên nhóm này");

        var existing = await rsvpRepo.GetAsync(r => r.EventId == request.EventId && r.UserId == callerId, ct);
        if (existing is null)
        {
            var rsvp = new Domain.Models.EventRsvp
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                UserId = callerId,
                Status = request.Status,
            };
            await rsvpRepo.AddAsync(rsvp, ct);
        }
        else
        {
            existing.Status = request.Status;
            await rsvpRepo.UpdateAsync(r => r.Id == existing.Id, existing, ct);
        }
        var ok = await rsvpRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Đã ghi nhận phản hồi" : "Thao tác thất bại");
    }
}
