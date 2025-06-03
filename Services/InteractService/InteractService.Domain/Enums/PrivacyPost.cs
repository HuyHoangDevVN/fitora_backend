namespace InteractService.Domain.Enums;

public enum PrivacyPost
{
    Private = 0,       // Chỉ mình người đăng có thể xem
    FriendsOnly = 1,   // Chỉ bạn bè có thể xem
    Public = 2,        // Bài đăng công khai
    GroupOnly = 3,     // Chỉ các thành viên trong nhóm được xem
    Custom = 4         // Quyền riêng tư tùy chỉnh
}
