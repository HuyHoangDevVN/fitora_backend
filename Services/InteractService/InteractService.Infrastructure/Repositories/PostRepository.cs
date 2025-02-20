using AutoMapper;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Services.IServices;
using InteractService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InteractService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IMapper _mapper;
    private readonly IAuthorizeExtension _authorizeExtension;
    private readonly DbSet<Post> _dbSet;

    public PostRepository(IRepositoryBase<Post> postRepo, IMapper mapper, IAuthorizeExtension authorizeExtension,
        ApplicationDbContext dbContext)
    {
        _postRepo = postRepo;
        _mapper = mapper;
        _authorizeExtension = authorizeExtension;
        _dbSet = dbContext.Set<Post>();
    }

    public async Task<bool> CreateAsync(Post post)
    {
        post.CreatedAt = DateTime.Now;
        post.CreatedBy = post.UserId;
        await _postRepo.AddAsync(post);
        return await _postRepo.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _postRepo.GetAllAsync();
    }

    public Task<IEnumerable<Post>> GetListAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        return await _postRepo.GetAsync(x => x.Id == id);
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        await _postRepo.UpdateAsync(p => p.Id == post.Id, post);
        return await _postRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await _postRepo.DeleteAsync(p => p.Id == id);
            return await _postRepo.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    public Task<IEnumerable<Post>> GetNewfeed()
    {
        throw new NotImplementedException();
    }

    public async Task<PaginatedCursorResult<Post>> GetPersonal(GetPostRequest request)
    {
        var query = _dbSet.Where(p => p.UserId == request.Id);

        // Nếu có cursor, chỉ lấy bài viết có CreatedAt < cursor
        if (request.Cursor.HasValue)
        {
            var cursorDateTime = DateTimeOffset.FromUnixTimeSeconds(request.Cursor.Value).UtcDateTime;
            query = query.Where(p => p.CreatedAt < cursorDateTime);
        }

        // Lấy danh sách với số lượng giới hạn + 1 để kiểm tra trang tiếp theo
        var items = await query
            .OrderByDescending(p => p.CreatedAt) // Thay vì cursorSelector, ta dùng trực tiếp trường CreatedAt
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xác định nextCursor nếu có nhiều hơn request.Limit bài viết
        long? nextCursor = items.Count > request.Limit
            ? new DateTimeOffset((DateTime)items.Last().CreatedAt!).ToUnixTimeSeconds()
            : null;

        // Đếm tổng số bài viết của user
        var totalCount = await _dbSet.CountAsync(p => p.UserId == request.Id);

        return new PaginatedCursorResult<Post>(
            cursor: request.Cursor,
            limit: request.Limit,
            count: totalCount,
            data: items.Take(request.Limit).ToList(),
            nextCursor: nextCursor
        );
    }
}