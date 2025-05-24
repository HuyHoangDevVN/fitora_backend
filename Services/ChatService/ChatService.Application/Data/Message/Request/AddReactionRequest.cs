namespace ChatService.Application.Data.Message.Request;

public record AddReactionRequest(string MessageId, string UserId, string Emoji);