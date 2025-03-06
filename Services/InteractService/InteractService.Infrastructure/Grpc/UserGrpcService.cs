using Google.Protobuf;
using InteractService.Domain.Enums;
using UserService.Infrastructure.Grpc;

namespace InteractService.Infrastructure.Grpc;

public class UserGrpcClient
{
    private readonly UserService.Infrastructure.Grpc.UserService.UserServiceClient _client;

    public UserGrpcClient(UserService.Infrastructure.Grpc.UserService.UserServiceClient client)
    {
        _client = client;
    }

    public static Guid ConvertByteStringToGuid(ByteString byteString)
    {
        byte[] bytes = byteString.ToByteArray();
        if (bytes.Length == 16)
        {
            return new Guid(bytes);
        }

        if (Guid.TryParse(byteString.ToStringUtf8(), out Guid guid))
        {
            return guid;
        }
        return Guid.Empty;
    }


    public async Task<List<UserInfo>> GetUserInfoBatchAsync(Guid? id, List<string> userIds)
    {
        
        var request = new GetUserInfoBatchRequest
        {
            Id = id.HasValue ? ByteString.CopyFrom(id.Value.ToByteArray()) : ByteString.Empty,
            UserIds = { userIds }
        };

        var response = await _client.GetUserInfoBatchAsync(request);
        return response.Users.Select(u => {
    
            return new UserInfo
            {
                Id = ConvertByteStringToGuid(u.Id),
                IsFriend = u.IsFriend,
                IsFollowing = u.IsFollowing,
                Email = u.Email,
                Username = u.Username,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Gender = Gender.TryParse(u.Gender, out Gender gender) ? gender : Gender.Unknown,
                BirthDate = Convert.ToDateTime(u.BirthDate),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Address = u.Address ?? string.Empty,
                ProfilePictureUrl = u.ProfilePictureUrl ?? string.Empty,
                Bio = u.Bio ?? string.Empty
            };
        }).ToList();
    }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public bool? IsFriend { get; set; }
    public bool? IsFollowing { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
}