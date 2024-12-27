namespace InteractService.Domain.Enums;

public enum ReportStatus
{
    Pending = 0,    // Chờ xử lý
    InProgress = 1, // Đang xử lý
    Resolved = 2,   // Đã giải quyết
    Closed = 3,     // Đã đóng
    Rejected = 4,   // Bị từ chối
    Archived = 5    // Đã lưu trữ
}
