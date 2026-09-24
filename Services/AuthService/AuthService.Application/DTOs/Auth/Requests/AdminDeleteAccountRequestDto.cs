namespace AuthService.Application.DTOs.Auth.Requests;

/// <summary>Request cho AdminController.DeleteAccount — chỉ cần UserId, không cần Password vì đã được bảo vệ bởi [Authorize(Roles = "ADMIN")].</summary>
public record AdminDeleteAccountRequestDto(string UserId);
