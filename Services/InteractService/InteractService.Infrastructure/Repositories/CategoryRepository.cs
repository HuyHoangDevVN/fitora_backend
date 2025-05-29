using System.Linq.Expressions;
using AutoMapper;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Category.Requests;
using InteractService.Application.DTOs.Category.Response;
using InteractService.Application.Services.IServices;
using InteractService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IRepositoryBase<Category> _categoryRepo;
    private readonly IRepositoryBase<FollowCategory> _categoryFollowRepo;
    private readonly DbSet<Category> _categories;
    private readonly DbSet<FollowCategory> _followCategories;
    private readonly DbSet<Post> _posts;
    private readonly IMapper _mapper;

    public CategoryRepository(IRepositoryBase<Category> categoryRepo,
        IRepositoryBase<FollowCategory> categoryFollowRepo, IMapper mapper, ApplicationDbContext dbContext)
    {
        _categoryRepo = categoryRepo;
        _categoryFollowRepo = categoryFollowRepo;
        _categories = dbContext.Set<Category>();
        _followCategories = dbContext.Set<FollowCategory>();
        _posts = dbContext.Set<Post>();
        _mapper = mapper;
    }

    public async Task<Category> CreateCategory(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(category.Slug))
        {
            category.Slug = category.Name.ToLower().Replace(" ", "-");
        }

        var existingCategory = await _categoryRepo.GetAsync(c => c.Slug == category.Slug);
        if (existingCategory != null)
        {
            throw new InvalidOperationException("Category đã tồn tại.");
        }

        await _categoryRepo.AddAsync(category);
        var isSuccess =  await _categoryRepo.SaveChangesAsync() > 0;
        return (isSuccess ? category : null)!;
    }

    public async Task<PaginatedResult<Category>> GetCategories(GetCategoriesRequest request)
    {
        var categories = await _categoryRepo.GetPageAsync(request, CancellationToken.None,
            c => request.KeySearch == null || c.Name.ToLower().Contains(request.KeySearch.ToLower()));
        return categories;
    }

    public Task<Category?> GetCategory(Guid id)
    {
        var category = _categoryRepo.GetAsync(c => c.Id == id);
        return category;
    }

    public async Task<ResponseDto> FollowCategoryAsync(FollowCategoryRequest request)
    {
        ValidateFollowCategoryRequest(request);

        if (await IsAlreadyFollowingCategoryAsync(request.CategoryId, request.UserId))
        {
            throw new InvalidOperationException("Bạn đã theo dõi danh mục này rồi!");
        }

        var followCategory = new FollowCategory
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow,
        };

        await _categoryFollowRepo.AddAsync(followCategory);
        var isSuccess = await _categoryFollowRepo.SaveChangesAsync() > 0;

        return new ResponseDto(
            Data: null,
            IsSuccess: isSuccess,
            Message: isSuccess ? "Theo dõi danh mục thành công!" : "Theo dõi danh mục thất bại."
        );
    }

    public async Task<ResponseDto> UnfollowCategoryAsync(FollowCategoryRequest request)
    {
        ValidateFollowCategoryRequest(request);

        if (!await IsAlreadyFollowingCategoryAsync(request.CategoryId, request.UserId))
        {
            throw new InvalidOperationException("Bạn chưa theo dõi danh mục này!");
        }

        await _categoryFollowRepo.DeleteAsync(fc => fc.CategoryId == request.CategoryId && fc.UserId == request.UserId);
        var isSuccess = await _categoryFollowRepo.SaveChangesAsync() > 0;
        return new ResponseDto(
            Data: null,
            IsSuccess: isSuccess,
            Message: isSuccess ? "Bỏ theo dõi danh mục thành công!" : "Bỏ theo dõi danh mục thất bại."
        );
    }


    public async Task<int> GetNumberOfFollowersAsync(Guid categoryId)
    {
        return await _categoryFollowRepo.CountAsync(fc => fc.CategoryId == categoryId);
    }

    public async Task<PaginatedResult<CategoryResponseDto>> GetCategoriesFollowed(GetCategoriesFollowedRequest request)
    {
        var includes = new List<Expression<Func<FollowCategory, object>>>
        {
            fc => fc.Category
        };

        Expression<Func<FollowCategory, bool>> conditions = fc =>
            fc.UserId == request.UserId &&
            (string.IsNullOrWhiteSpace(request.KeySearch) ||
             fc.Category.Name.ToLower().Contains(request.KeySearch.ToLower()));

        return await _categoryFollowRepo.GetPageWithIncludesAsync(
            paginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
            selector: fc => _mapper.Map<CategoryResponseDto>(fc.Category),
            conditions: conditions,
            includes: includes,
            cancellationToken: CancellationToken.None
        );
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetTrendingCategories(int limit = 10,
        TimeSpan? timeRange = null)
    {
        timeRange ??= TimeSpan.FromDays(7); // Mặc định 7 ngày

        // Truy vấn chính
        var trendingCategories = await _categories
            .Select(c => new
            {
                Category = c,
                FollowerCount = _followCategories
                    .Count(fc => fc.CategoryId == c.Id), // Đếm số người theo dõi
                PostCount = _posts
                    .Count(p => p.CategoryId == c.Id &&
                                p.CreatedAt >= DateTime.UtcNow - timeRange), // Đếm bài viết trong khoảng thời gian
            })
            .Select(x => new
            {
                x.Category,
                x.FollowerCount,
                x.PostCount,
                TrendScore = (double)(x.FollowerCount * 2 + x.PostCount) // Công thức xu hướng
            })
            .OrderByDescending(x => x.TrendScore)
            .Take(limit)
            .Select(x => new CategoryResponseDto
            {
                Id = x.Category.Id,
                Name = x.Category.Name,
                FollowerCount = x.FollowerCount,
                PostCount = x.PostCount,
                TrendScore = x.TrendScore
            })
            .ToListAsync();

        return trendingCategories;
    }

    private void ValidateFollowCategoryRequest(FollowCategoryRequest request)
    {
        if (request.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("CategoryId cannot be empty", nameof(request.CategoryId));
        }

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(request.UserId));
        }
    }

    private async Task<bool> IsAlreadyFollowingCategoryAsync(Guid categoryId, Guid userId)
    {
        return await _categoryFollowRepo.GetAsync(fc => fc.CategoryId == categoryId && fc.UserId == userId) != null;
    }
}