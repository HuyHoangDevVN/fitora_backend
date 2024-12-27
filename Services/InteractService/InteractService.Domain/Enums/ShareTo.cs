namespace InteractService.Domain.Enums;

public enum ShareTo
{
    Everyone = 0,    // Chia sẻ cho tất cả mọi người
    Friends = 1,     // Chia sẻ cho bạn bè
    Group = 2,       // Chia sẻ cho nhóm
    SpecificUsers = 3, // Chia sẻ cho những người dùng cụ thể
    Private = 4      // Chia sẻ cho người đăng (riêng tư)
}
