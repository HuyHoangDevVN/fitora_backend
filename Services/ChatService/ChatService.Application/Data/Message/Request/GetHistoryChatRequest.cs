using BuildingBlocks.Pagination.Base;

namespace ChatService.Application.Data.Message.Request;

public record GetHistoryChatRequest(string ConversationId, int PageIndex = 0, int PageSize = 10): PaginationRequest;