namespace AuthService.Application.Auths.Commands.AdminDeleteAccount;

/// <summary>Admin xóa tài khoản người dùng khác — không cần password, đã được bảo vệ bởi [Authorize(Roles = "ADMIN")] ở AdminController.</summary>
public record AdminDeleteAccountCommand(string UserId) : ICommand<AdminDeleteAccountResult>;

public record AdminDeleteAccountResult(bool IsSuccess);
