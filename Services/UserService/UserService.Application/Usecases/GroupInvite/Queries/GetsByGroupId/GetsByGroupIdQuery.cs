using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetsByGroupId;

public record GetsByGroupIdQuery(GetGroupInvitesRequest Request) : IQuery<ResponseDto>;