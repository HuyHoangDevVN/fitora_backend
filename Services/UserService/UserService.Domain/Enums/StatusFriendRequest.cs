namespace UserService.Domain.Enums;

public enum StatusFriendRequest
{
    Pending = 0,        // Đang chờ xử lý
    Accepted = 1,       // Đã được chấp nhận
    Rejected = 2,       // Đã bị từ chối
    Cancelled = 3,      // Đã bị hủy
    Blocked = 4         // Đã bị chặn
}
