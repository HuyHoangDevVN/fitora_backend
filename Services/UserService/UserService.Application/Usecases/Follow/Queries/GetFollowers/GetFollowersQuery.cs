using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Usecases.Follow.Queries.GetFollowers;

public record GetFollowersQuery(GetFollowersRequest Request): IQuery<ResponseDto>;