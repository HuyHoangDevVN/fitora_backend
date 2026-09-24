using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Usecases.GroupEvent.Queries.GetGroupEvents;

public class GetGroupEventsHandler(IRepositoryBase<Domain.Models.GroupEvent> eventRepo)
    : IQueryHandler<GetGroupEventsQuery, PaginatedResult<Domain.Models.GroupEvent>>
{
    public async Task<PaginatedResult<Domain.Models.GroupEvent>> Handle(
        GetGroupEventsQuery request, CancellationToken ct)
    {
        return await eventRepo.GetPageAsync(
            new PaginationRequest(request.PageIndex, request.PageSize), ct,
            e => e.GroupId == request.GroupId);
    }
}
