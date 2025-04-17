using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Queries.GetByGroupId;

public record GetMembersByGroupIdQuery(GetByGroupIdRequest Request) : IQuery<PaginatedResult<GroupMemberDto>>;