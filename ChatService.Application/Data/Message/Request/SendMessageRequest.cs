namespace ChatService.Application.Data.Message.Request;

public record SendMessageRequest(string ConversationId, string Content, string Type);