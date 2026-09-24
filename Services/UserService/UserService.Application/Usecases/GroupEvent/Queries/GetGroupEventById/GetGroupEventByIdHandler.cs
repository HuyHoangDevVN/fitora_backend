using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Usecases.GroupEvent.Queries.GetGroupEventById;

public class GetGroupEventByIdHandler(IRepositoryBase<Domain.Models.GroupEvent> eventRepo)
    : IQueryHandler<GetGroupEventByIdQuery, Domain.Models.GroupEvent>
{
    public async Task<Domain.Models.GroupEvent> Handle(GetGroupEventByIdQuery request, CancellationToken ct)
    {
        return await eventRepo.GetAsync(e => e.Id == request.EventId, ct)
            ?? throw new NotFoundException("Sự kiện không tồn tại");
    }
}
