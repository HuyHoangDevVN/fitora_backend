using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Requests;

namespace UserService.Application.Usecases.Friendship.Queries.GetFriendRequests.Received;

public record GetReceivedFriendRequestQuerry(GetReceivedFriendRequest Request) : IQuery<ResponseDto>;