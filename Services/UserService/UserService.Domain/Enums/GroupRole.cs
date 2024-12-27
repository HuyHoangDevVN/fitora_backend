namespace UserService.Domain.Enums;

public enum GroupRole
{
    Owner = 1,       // Chủ sở hữu nhóm
    Admin = 2,       // Quản trị viên của nhóm
    Moderator = 3,   // Người kiểm duyệt nội dung
    Member = 4,      // Thành viên thông thường
}
