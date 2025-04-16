using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetManagedGroups;

public record GetManagedGroupsQuery(GetManagedGroupsRequest Request) : IQuery<PaginatedResult<GroupDto>>;