namespace ChatService.Application.Data.Message.Request;

public record CreateConversationRequest(List<string> ParticipantIds, bool IsGroup = false, Group? GroupInfo = null);