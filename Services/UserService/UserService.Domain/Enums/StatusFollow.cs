namespace UserService.Domain.Enums;

public enum StatusFollow
{
    Pending = 1,        // Đang chờ chấp nhận (nếu cần phê duyệt theo dõi)
    Following = 2,      // Đã theo dõi
    FollowedBy = 3,     // Được người khác theo dõi
    Blocked = 4,        // Đã bị chặn
    Unfollowed = 5      // Đã hủy theo dõi
}
