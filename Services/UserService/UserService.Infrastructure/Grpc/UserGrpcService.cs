using Google.Protobuf;
using Grpc.Core;
using UserService.Application.Services;
using UserService.Application.Services.IServices;
using UserService.Infrastructure.Grpc;
using GetUserRequest = UserService.Infrastructure.Grpc.GetUserRequest;

public class UserGrpcService : UserService.Infrastructure.Grpc.UserService.UserServiceBase
{
   private readonly IUserRepository _userRepository;

    public UserGrpcService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var userRequest = new UserService.Application.DTOs.User.Requests.GetUserRequest(
            Id: request.Id.Length > 0 ? new Guid(request.Id.ToByteArray()) : Guid.Empty,
            GetId: request.GetId.Length > 0 ? new Guid(request.GetId.ToByteArray()) : null,
            Email: request.Email ?? string.Empty,
            Username: request.Username ?? string.Empty
        );

        var user = await _userRepository.GetUser(userRequest);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        return new GetUserResponse
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Username = user.Username,
            FirstName = user.UserInfo?.FirstName ?? string.Empty,
            LastName = user.UserInfo?.LastName ?? string.Empty,
            Gender = user.UserInfo?.Gender.ToString() ?? "Unknown",
            BirthDate = user.UserInfo?.BirthDate?.ToString("o") ?? string.Empty,
            PhoneNumber = user.UserInfo?.PhoneNumber ?? string.Empty,
            Address = user.UserInfo?.Address ?? string.Empty,
            ProfilePictureUrl = user.UserInfo?.ProfilePictureUrl ?? string.Empty,
            Bio = user.UserInfo?.Bio ?? string.Empty
        };
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var usersRequest = new UserService.Application.DTOs.User.Requests.GetUsersRequest(
            Username: request.Username ?? string.Empty,
            Email: request.Email ?? string.Empty
        )
        {
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };

        var paginatedResult = await _userRepository.GetUsers(usersRequest);

        var response = new GetUsersResponse
        {
            PageIndex = paginatedResult.PageIndex,
            PageSize = paginatedResult.PageSize,
            TotalCount = (int)paginatedResult.Count 
        };

        response.Data.AddRange(paginatedResult.Data.Select(u => new UserWithInfoDto
        {
            Id = ByteString.CopyFrom(u.Id.ToByteArray()),
            Email = u.Email ?? string.Empty,
            Username = u.Username ?? string.Empty,
            FirstName = u.FirstName ?? string.Empty,
            LastName = u.LastName ?? string.Empty,
            Gender = u.Gender.ToString() ?? "Unknown",
            BirthDate = u.BirthDate.ToString("o"),
            PhoneNumber = u.PhoneNumber ?? string.Empty,
            Address = u.Address ?? string.Empty,
            ProfilePictureUrl = u.ProfilePictureUrl ?? string.Empty,
            Bio = u.Bio ?? string.Empty
        }));

        return response;
    }

    public override async Task<GetUserInfoBatchResponse> GetUserInfoBatch(GetUserInfoBatchRequest request, ServerCallContext context)
    {
        var userIds = request.UserIds
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var users = await _userRepository.GetUsersByIdsAsync(userIds);

        var response = new GetUserInfoBatchResponse();
        response.Users.AddRange(users.Select(u => new GetUserInfoResponse
        {
            Id = ByteString.CopyFrom(u.Id.ToByteArray()),
            Email = u.Email ?? string.Empty,
            Username = u.Username ?? string.Empty,
            FirstName = u.FirstName ?? string.Empty,
            LastName = u.LastName ?? string.Empty,
            Gender = u.Gender.ToString() ?? "Unknown",
            BirthDate = u.BirthDate.ToString("o"),
            PhoneNumber = u.PhoneNumber ?? string.Empty,
            Address = u.Address ?? string.Empty,
            ProfilePictureUrl = u.ProfilePictureUrl ?? string.Empty,
            Bio = u.Bio ?? string.Empty
        }));

        return response;
    }
}