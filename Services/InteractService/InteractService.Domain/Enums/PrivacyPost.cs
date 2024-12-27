namespace InteractService.Domain.Enums;

public enum PrivacyPost
{
    Public = 0,        // Bài đăng công khai
    FriendsOnly = 1,   // Chỉ bạn bè có thể xem
    Private = 2,       // Chỉ mình người đăng có thể xem
    GroupOnly = 3,     // Chỉ các thành viên trong nhóm được xem
    Custom = 4         // Quyền riêng tư tùy chỉnh
}
