using AuthService.Application.DTOs.Users.Responses;

namespace AuthService.Application.Extensions;

public static class AuthExtensions
{
    public static GetUserResponseDto ApplicationUserToUserInFo(ApplicationUser user, IList<string> roles = null)
        => new GetUserResponseDto(user.Id, user.Email ?? "", user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar, user.Status, roles);
    
    
}