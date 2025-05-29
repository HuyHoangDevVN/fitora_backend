namespace InteractService.Application.DTOs.Category.Requests;

public record GetTrendingCategoriesRequest(
    int Limit = 10,
    TimeSpan? TimeRange = null);