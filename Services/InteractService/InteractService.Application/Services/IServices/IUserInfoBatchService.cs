namespace InteractService.Application.Services.IServices;

/// <summary>
/// Trả thông tin hiển thị (username/avatar) cho 1 danh sách userId — dùng để enrich
/// DTO không có navigation property tới User (ví dụ ShortClip chỉ lưu AuthorId).
/// </summary>
public interface IUserInfoBatchService
{
    Task<Dictionary<Guid, UserDisplayInfo>> GetUserDisplayInfosAsync(Guid? requestUserId, List<Guid> userIds, CancellationToken cancellationToken);
}

public record UserDisplayInfo(Guid Id, string Username, string? ProfilePictureUrl);
