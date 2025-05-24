namespace ChatService.Application.Data.Message.Request;

public record GroupMemberRequest(string ConversationId, string UserId);