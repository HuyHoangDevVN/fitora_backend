using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupMember.Responses;

namespace UserService.Application.Usecases.GroupMember.Queries.GetById;

public record GetMemberByIdQuery(Guid Id, Guid GroupId) : IQuery<GroupMemberDto>;