namespace UserService.Application.DTOs.Search.Responses;

public record RecentSearchDto(Guid Id, string Query, DateTime SearchedAt);
