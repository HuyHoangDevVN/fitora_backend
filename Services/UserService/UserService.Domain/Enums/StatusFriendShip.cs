namespace UserService.Domain.Enums;

public enum StatusFriendShip
{
    Friends = 1,        // Hai người đã là bạn bè
    Blocked = 2,        // Một trong hai người đã chặn người kia
    Pending = 3,        // Yêu cầu kết bạn đang chờ xử lý
    Cancelled = 4,      // Yêu cầu kết bạn đã bị hủy
    Rejected = 5        // Yêu cầu kết bạn đã bị từ chối
}
