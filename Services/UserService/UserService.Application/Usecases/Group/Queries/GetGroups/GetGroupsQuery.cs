using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;

namespace UserService.Application.Usecases.Group.Queries.GetGroups;

public record GetGroupsQuery(GetGroupsRequest Request) : IQuery<PaginatedResult<Domain.Models.Group>>;