using UserService.Domain.Enums;

namespace UserService.Application.DTOs.User.Requests;

public record UpdateUserRequest(Guid Id, string? Username, string? Email, UserInfoDetail? UserInfo);

public record UserInfoDetail(
    string? FirstName,
    string? LastName,
    DateTime? Birthday,
    Gender? Gender,
    string? Address,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    string? Bio);