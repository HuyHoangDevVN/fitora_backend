using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Sended;

public record GetSentFriendRequestQuerry(GetSentFriendRequest Request) : IQuery<ResponseDto>;