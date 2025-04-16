using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetJoinedGroups;

public record GetJoinedGroupsQuery(GetJoinedGroupsRequest Request) : IQuery<PaginatedResult<GroupDto>>;