using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Category.Response;

namespace InteractService.Application.Services.IServices;

public interface ICategoryRepository
{
    Task<Category> CreateCategory(Category category);
    Task<bool> UpdateCategory(Category category);
    Task<PaginatedResult<Category>> GetCategories(GetCategoriesRequest request);
    Task<Category?> GetCategory(Guid id);
    Task<ResponseDto> FollowCategoryAsync(FollowCategoryRequest request);
    Task<ResponseDto> UnfollowCategoryAsync(FollowCategoryRequest request);
    Task<int> GetNumberOfFollowersAsync(Guid categoryId);
    Task<PaginatedResult<CategoryResponseDto>> GetCategoriesFollowed(GetCategoriesFollowedRequest request);

    Task<IEnumerable<CategoryResponseDto>> GetTrendingCategories(int limit = 10,
        TimeSpan? timeRange = null);
    
    Task<bool> DeleteCategoryAsync(Guid id);
}