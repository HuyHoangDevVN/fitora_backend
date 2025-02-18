using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Follow.Requests;

namespace UserService.Application.Usecases.Follow.Queries.GetFollowees;

public record GetFolloweesQuerry(GetFollowersRequest Request): IQuery<ResponseDto>;