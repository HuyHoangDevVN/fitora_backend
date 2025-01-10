using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriends;

public record GetFriendsQuerry(GetFriendsRequest Request): IQuery<ResponseDto>;