using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;

namespace UserService.Application.Usecases.GroupEvent.Queries.GetGroupEvents;

public record GetGroupEventsQuery(Guid GroupId, int PageIndex = 0, int PageSize = 20) : IQuery<PaginatedResult<Domain.Models.GroupEvent>>;
