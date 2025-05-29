namespace ChatService.Application.Data.Message.Request;

public record UpdateGroupInfoRequest(string ConversationId, Group GroupInfo);