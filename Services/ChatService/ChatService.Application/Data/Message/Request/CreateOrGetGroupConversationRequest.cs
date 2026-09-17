namespace ChatService.Application.Data.Message.Request;

public record CreateOrGetGroupConversationRequest(string? GroupName, List<string>? MemberIds);

public record SyncGroupMembersRequest(List<string> MemberIds);
