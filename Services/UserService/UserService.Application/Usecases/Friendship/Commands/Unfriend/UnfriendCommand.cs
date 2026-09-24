namespace UserService.Application.Usecases.Friendship.Commands.Unfriend;

// 27.x: phải mang cả CurrentUserId để chỉ xóa đúng quan hệ giữa 2 người —
// trước đây chỉ có TargetUserId nên bất kỳ user nào cũng xóa được TOÀN BỘ
// bạn bè của người khác (chỉ cần biết id của họ).
public record UnfriendCommand(Guid CurrentUserId, Guid TargetUserId) : ICommand<bool>;