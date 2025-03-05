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
using StackExchange.Redis;

namespace InteractService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IMapper _mapper;
    private readonly IAuthorizeExtension _authorizeExtension;
    private readonly DbSet<Post> _dbSet;
    private readonly IDatabase _redis;
    private readonly UserGrpcClient _userClient;

    public PostRepository(IRepositoryBase<Post> postRepo, IMapper mapper, IAuthorizeExtension authorizeExtension,
        ApplicationDbContext dbContext, IConnectionMultiplexer redis, UserGrpcClient userClient)
    {
        _postRepo = postRepo;
        _mapper = mapper;
        _authorizeExtension = authorizeExtension;
        _dbSet = dbContext.Set<Post>();
        _redis = redis.GetDatabase();
        _userClient = userClient;
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

    // public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    // {
    //     var now = DateTime.UtcNow;
    //     IQueryable<Post> query = _dbSet;
    //
    //     // Nếu có composite cursor, parse chuỗi để lấy lastScore và lastCreatedAt.
    //     double? lastScore = null;
    //     DateTime? lastCreatedAt = null;
    //     if (!string.IsNullOrEmpty(request.Cursor))
    //     {
    //         var parts = request.Cursor.Split('|');
    //         if (parts.Length == 2 &&
    //             double.TryParse(parts[0], out var parsedScore) &&
    //             long.TryParse(parts[1], out var ticks))
    //         {
    //             lastScore = parsedScore;
    //             lastCreatedAt = new DateTime(ticks, DateTimeKind.Utc);
    //         }
    //     }
    //
    //     // Áp dụng điều kiện lọc dựa trên composite cursor nếu có
    //     if (lastScore.HasValue && lastCreatedAt.HasValue)
    //     {
    //         query = query.Where(p =>
    //             ((p.VotesCount * 2 + p.CommentsCount) / ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)) <
    //             lastScore.Value
    //             ||
    //             (
    //                 ((p.VotesCount * 2 + p.CommentsCount) /
    //                  ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)) == lastScore.Value
    //                 && p.CreatedAt < lastCreatedAt.Value
    //             )
    //         );
    //     }
    //
    //     // Tính Score cho mỗi bài viết và sắp xếp theo Score giảm dần, sau đó theo CreatedAt giảm dần
    //     var postsWithScore = await query
    //         .Select(p => new
    //         {
    //             Post = p,
    //             Score = (p.VotesCount * 2 + p.CommentsCount) /
    //                     ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)
    //         })
    //         .OrderByDescending(x => x.Score)
    //         .ThenByDescending(x => x.Post.CreatedAt)
    //         .Take(request.Limit + 1)
    //         .ToListAsync();
    //
    //     // Xác định nextCursor nếu số lượng trả về vượt quá limit
    //     string nextCursor = null;
    //     if (postsWithScore.Count > request.Limit)
    //     {
    //         var lastItem = postsWithScore[request.Limit];
    //         // Ép kiểu CreatedAt thành DateTime để gọi thuộc tính Ticks
    //         nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}";
    //         postsWithScore = postsWithScore.Take(request.Limit).ToList();
    //     }
    //
    //     // Map dữ liệu sang PostResponseDto
    //     var postDtos = postsWithScore.Select(x => new PostResponseDto
    //     {
    //         Id = x.Post.Id,
    //         Content = x.Post.Content,
    //         CreatedAt = (DateTimeOffset)x.Post.CreatedAt,
    //         VotesCount = x.Post.VotesCount,
    //         CommentsCount = x.Post.CommentsCount,
    //         Score = x.Score,
    //         MediaUrl = x.Post.MediaUrl,
    //         Privacy = x.Post.Privacy,
    //         GroupId = x.Post.GroupId,
    //         UserId = x.Post.UserId,
    //         IsDeleted = x.Post.IsDeleted
    //     }).ToList();
    //
    //     // Đếm tổng số bài viết thỏa điều kiện (theo query đã được lọc)
    //     var totalCount = await query.CountAsync();
    //
    //     return new PaginatedCursorResult<PostResponseDto>(
    //         request.Cursor,
    //         request.Limit,
    //         totalCount,
    //         postDtos,
    //         nextCursor
    //     );
    // }

    public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSet;

        // Parse composite cursor nếu có
        double? lastScore = null;
        DateTime? lastCreatedAt = null;
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            var parts = request.Cursor.Split('|');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out var parsedScore) &&
                long.TryParse(parts[1], out var ticks))
            {
                lastScore = parsedScore;
                lastCreatedAt = new DateTime(ticks, DateTimeKind.Utc);
            }
        }

        // Áp dụng điều kiện lọc dựa trên composite cursor nếu có
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query.Where(p =>
                ((p.VotesCount * 2 + p.CommentsCount) / ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)) <
                lastScore.Value
                ||
                (
                    ((p.VotesCount * 2 + p.CommentsCount) /
                     ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)) == lastScore.Value
                    && p.CreatedAt < lastCreatedAt.Value
                )
            );
        }

        // Tính Score và sắp xếp
        var postsWithScore = await query
            .Select(p => new
            {
                Post = p,
                Score = (p.VotesCount * 2 + p.CommentsCount) /
                        ((EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0) + 2)
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Post.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xác định nextCursor
        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }

        // Lấy danh sách userIds từ posts, dùng định dạng "D" cho Guid
        var userIds = postsWithScore.Select(x => x.Post.UserId.ToString("D")).Distinct().ToList();
        var userInfos = new Dictionary<string, UserWithInfoDto>();

        // Lấy thông tin người dùng từ Redis
        foreach (var id in userIds)
        {
            var redisKey = $"user_info:{id}";
            var userData = await _redis.HashGetAllAsync(redisKey);
            if (userData.Length > 0)
            {
                userInfos[id] = new UserWithInfoDto
                {
                    Id = Guid.TryParse(id, out var guid) ? guid : Guid.Empty,
                    Username = userData.FirstOrDefault(x => x.Name == "username").Value,
                    FirstName = userData.FirstOrDefault(x => x.Name == "first_name").Value,
                    LastName = userData.FirstOrDefault(x => x.Name == "last_name").Value,
                    ProfilePictureUrl = userData.FirstOrDefault(x => x.Name == "profile_picture_url").Value
                    // Thêm các trường khác nếu cần
                };
            }
        }

        // Fallback sang gRPC nếu cache miss
        var missingIds = userIds.Except(userInfos.Keys).ToList();
        if (missingIds.Any())
        {
            var grpcUsers = await _userClient.GetUserInfoBatchAsync(missingIds);
            foreach (var user in grpcUsers)
            {
                // Sử dụng định dạng "D" cho key để đảm bảo nhất quán
                var key = user.Id.ToString("D");
                var userDto = new UserWithInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                };
                userInfos[key] = userDto;

                // Lưu vào Redis
                await _redis.HashSetAsync($"user_info:{key}", new[]
                {
                    new HashEntry("username", userDto.Username ?? string.Empty),
                    new HashEntry("first_name", userDto.FirstName ?? string.Empty),
                    new HashEntry("last_name", userDto.LastName ?? string.Empty),
                    new HashEntry("profile_picture_url", userDto.ProfilePictureUrl ?? string.Empty)
                });
            }
        }

        // Map dữ liệu sang PostResponseDto
        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt,
            VotesCount = x.Post.VotesCount,
            CommentsCount = x.Post.CommentsCount,
            Score = x.Score,
            MediaUrl = x.Post.MediaUrl,
            Privacy = x.Post.Privacy,
            GroupId = x.Post.GroupId,
            UserId = x.Post.UserId,
            IsDeleted = x.Post.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Post.UserId.ToString("D"))
        }).ToList();

        // Đếm tổng số bài viết
        var totalCount = await query.CountAsync();

        return new PaginatedCursorResult<PostResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            postDtos,
            nextCursor
        );
    }

    public async Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request)
    {
        // Lấy bài viết của user theo Id
        IQueryable<Post> query = _dbSet.Where(p => p.UserId == request.Id);

        // Nếu có composite cursor, parse để lấy lastCreatedAt
        DateTime? lastCreatedAt = null;
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            var parts = request.Cursor.Split('|');
            if (parts.Length == 2 && long.TryParse(parts[1], out var ticks))
            {
                lastCreatedAt = new DateTime(ticks, DateTimeKind.Utc);
            }
        }

        // Nếu có cursor, chỉ lấy bài viết có CreatedAt < lastCreatedAt (tức bài viết cũ hơn)
        if (lastCreatedAt.HasValue)
        {
            query = query.Where(p => p.CreatedAt < lastCreatedAt.Value);
        }

        // Sắp xếp theo CreatedAt giảm dần và lấy thêm 1 phần tử để kiểm tra trang tiếp theo
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xác định nextCursor nếu số lượng trả về vượt quá limit
        string nextCursor = null;
        if (items.Count > request.Limit)
        {
            var lastItem = items[request.Limit];
            // Tạo composite cursor: Score = 0 vì không tính điểm, và CreatedAt.Ticks
            nextCursor = $"0|{((DateTime)lastItem.CreatedAt).Ticks}";
            items = items.Take(request.Limit).ToList();
        }

        // Đếm tổng số bài viết của user
        var totalCount = await _dbSet.CountAsync(p => p.UserId == request.Id);

        // Map dữ liệu sang PostResponseDto, gán Score = 0
        var postDtos = items.Select(p => new PostResponseDto
        {
            Id = p.Id,
            Content = p.Content,
            CreatedAt = (DateTimeOffset)p.CreatedAt,
            VotesCount = p.VotesCount,
            CommentsCount = p.CommentsCount,
            Score = 0,
            MediaUrl = p.MediaUrl,
            Privacy = p.Privacy,
            GroupId = p.GroupId,
            UserId = p.UserId,
            IsDeleted = p.IsDeleted
        }).ToList();

        return new PaginatedCursorResult<PostResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            postDtos,
            nextCursor
        );
    }
}