namespace AuthService.Application.DTOs.Users.Requests;

public record EditInfoUserRequest(string UserId,
    string FullName,
    string PhoneNumber,
    string Avatar);