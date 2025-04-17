using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupInvite.Requests;

namespace UserService.Application.Usecases.GroupInvite.Queries.GetSentList;

public record GetSentListQuery(GetSentGroupInviteRequest Request) : IQuery<ResponseDto>;