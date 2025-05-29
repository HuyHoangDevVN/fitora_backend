namespace ChatService.Application.Data.Message.Request;

public record MarkAsReadRequest(string MessageId, bool IsRead);