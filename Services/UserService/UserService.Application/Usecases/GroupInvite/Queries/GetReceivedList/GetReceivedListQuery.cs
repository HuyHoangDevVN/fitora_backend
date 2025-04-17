using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetReceivedList;

public record GetReceivedListQuery(GetReceivedGroupInviteRequest Request) : IQuery<ResponseDto>;