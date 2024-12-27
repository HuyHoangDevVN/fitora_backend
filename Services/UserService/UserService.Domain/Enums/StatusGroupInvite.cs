namespace UserService.Domain.Enums;

public enum StatusGroupInvite
{
    Pending = 1,    // Lời mời đang chờ được chấp nhận
    Accepted = 2,   // Lời mời đã được chấp nhận
    Declined = 3,   // Lời mời bị từ chối
    Revoked = 4,    // Lời mời bị hủy bởi người gửi
    Expired = 5     // Lời mời đã hết hạn
}