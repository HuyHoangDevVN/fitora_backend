using System.Linq.Expressions;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Services.IServices;
using InteractService.Domain.Enums;
using InteractService.Infrastructure.Data;
using InteractService.Infrastructure.Grpc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace InteractService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IRepositoryBase<UserVoted> _userVotedRepo;
    private readonly DbSet<Post> _dbSetPosts;
    private readonly DbSet<UserVoted> _dbSetUserVoteds;
    private readonly DbSet<Comment> _dbSetComments;
    private readonly DbSet<Category> _dbSetCategories;
    private readonly DbSet<FollowCategory> _dbSetFollowCategories;
    private readonly IDatabase _redis;
    private readonly UserGrpcClient _userClient;

    public PostRepository(IRepositoryBase<Post> postRepo, IRepositoryBase<UserVoted> userVotedRepo,
        ApplicationDbContext dbContext, IConnectionMultiplexer redis, UserGrpcClient userClient)
    {
        _postRepo = postRepo;
        _dbSetPosts = dbContext.Set<Post>();
        _dbSetUserVoteds = dbContext.Set<UserVoted>();
        _dbSetComments = dbContext.Set<Comment>();
        _dbSetCategories = dbContext.Set<Category>();
        _dbSetFollowCategories = dbContext.Set<FollowCategory>();
        _redis = redis.GetDatabase();
        _userClient = userClient;
        _userVotedRepo = userVotedRepo;
    }

    public async Task<bool> CreateAsync(Post post)
    {
        post.CreatedAt = DateTime.Now;
        post.CreatedBy = post.UserId;
        await _postRepo.AddAsync(post);
        return await _postRepo.SaveChangesAsync() > 0;
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        return (await _postRepo.GetAsync(x => x.Id == id))!;
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        await _postRepo.UpdateAsync(p => p.Id == post.Id, post);
        return await _postRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> VoteAsync(VotePostRequest request)
    {
        var post = await _postRepo.GetAsync(p => p.Id == request.PostId);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        var existingVote =
            await _userVotedRepo.GetAsync(uv => uv.UserId == request.UserId && uv.PostId == request.PostId);

        if (request.VoteType == VoteType.UnVote)
        {
            // Nếu là UnVote và có bản ghi vote, xóa bản ghi
            if (existingVote != null)
            {
                await _userVotedRepo.DeleteAsync(uv => uv.Id == existingVote.Id);
            }
        }
        else
        {
            if (existingVote != null)
            {
                // Cập nhật VoteType
                existingVote.VoteType = request.VoteType;
                await _userVotedRepo.UpdateAsync(uv => uv.Id == existingVote.Id, existingVote);
            }
            else
            {
                await _userVotedRepo.AddAsync(new UserVoted
                {
                    UserId = request.UserId,
                    PostId = request.PostId,
                    VoteType = request.VoteType
                });
            }
        }

        return await _userVotedRepo.SaveChangesAsync() > 0;
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts.Where(p => p.UserId == request.Id);

        // Parse composite cursor nếu có
        (double? lastScore, DateTime? lastCreatedAt) = ParseCursor(request.Cursor ?? string.Empty);

        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            Expression<Func<Post, bool>> filter = p =>
                GetPostScore(p, now) < lastScore.Value ||
                (GetPostScore(p, now) == lastScore.Value && p.CreatedAt < lastCreatedAt.Value);
            query = query.Where(filter);
        }

        // Tính VotesCount và Score tương tự GetNewfeed
        var postsWithScore = await query
            .Select(p => new
            {
                Post = p,
                VotesCount = _dbSetUserVoteds
                    .Where(uv => uv.PostId == p.Id)
                    .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 : uv.VoteType == VoteType.DownVote ? -1 : 0),
                CommentsCount = _dbSetComments
                    .Count(c => c.PostId == p.Id && !c.IsDeleted),
                HoursSinceCreation = EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2,
                CategoryName = _dbSetCategories
                    .Where(cat => cat.Id == p.CategoryId)
                    .Select(cat => cat.Name)
                    .FirstOrDefault(),
                IsCategoryFollowed = p.CategoryId.HasValue && 
                                     _dbSetFollowCategories
                                         .Any(fc => fc.UserId == request.Id && fc.CategoryId == p.CategoryId.Value)
            })
            .Select(x => new
            {
                x.Post,
                x.VotesCount,
                x.CommentsCount,
                x.HoursSinceCreation,
                x.CategoryName,
                x.IsCategoryFollowed,
                Score = (x.VotesCount * 2 + x.Post.CommentsCount) / x.HoursSinceCreation
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Post.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xử lý con trỏ (cursor) cho trang tiếp theo
        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }

        // Lấy danh sách PostId
        var postIds = postsWithScore.Select(x => x.Post.Id).ToList();

        // Lấy vote của người dùng cho các bài viết này
        var userVotes = await _dbSetUserVoteds
            .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
            .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);

        var totalCount = await query.CountAsync();

        // Lấy thông tin người dùng
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
                    Username = userData.FirstOrDefault(x => x.Name == "username").Value!,
                    Email = userData.FirstOrDefault(x => x.Name == "email").Value!,
                    IsFriend = (bool?)userData.FirstOrDefault(x => x.Name == "is_friend").Value,
                    IsFollowing = (bool?)userData.FirstOrDefault(x => x.Name == "is_following").Value,
                    FirstName = userData.FirstOrDefault(x => x.Name == "first_name").Value,
                    LastName = userData.FirstOrDefault(x => x.Name == "last_name").Value,
                    ProfilePictureUrl = userData.FirstOrDefault(x => x.Name == "profile_picture_url").Value,
                    Address = userData.FirstOrDefault(x => x.Name == "address").Value,
                    Gender = Enum.TryParse<Gender>(userData.FirstOrDefault(x => x.Name == "gender").Value,
                        out var gender)
                        ? gender
                        : Gender.Unknown,
                    BirthDate = DateTime.TryParse(userData.FirstOrDefault(x => x.Name == "birth_date").Value,
                        out var birthDate)
                        ? birthDate
                        : default,
                    PhoneNumber = userData.FirstOrDefault(x => x.Name == "phone_number").Value,
                };
            }
        }

        // Fallback sang gRPC nếu cache miss
        var missingIds = userIds.Except(userInfos.Keys).ToList();
        if (missingIds.Any())
        {
            var grpcUsers = await _userClient.GetUserInfoBatchAsync(request.Id, missingIds);
            foreach (var user in grpcUsers)
            {
                var key = user.Id.ToString("D");
                var userDto = new UserWithInfoDto
                {
                    Id = user.Id,
                    IsFriend = user.IsFriend,
                    IsFollowing = user.IsFollowing,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Bio = user.Bio,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber,
                };
                userInfos[key] = userDto;
                // Lưu vào Redis
                await _redis.HashSetAsync($"user_info:{key}", new[]
                {
                    new HashEntry("username", userDto.Username),
                    new HashEntry("email", userDto.Email),
                    new HashEntry("is_friend", userDto.IsFriend),
                    new HashEntry("is_following",userDto.IsFollowing),
                    new HashEntry("first_name", userDto.FirstName ?? string.Empty),
                    new HashEntry("last_name", userDto.LastName ?? string.Empty),
                    new HashEntry("profile_picture_url", userDto.ProfilePictureUrl ?? string.Empty),
                    new HashEntry("address", userDto.Address ?? string.Empty),
                    new HashEntry("gender", userDto.Gender.ToString()),
                    new HashEntry("birth_date", userDto.BirthDate.ToString("yyyy-MM-dd")),
                    new HashEntry("phone_number", userDto.PhoneNumber ?? string.Empty)
                });
            }
        }

        // Map dữ liệu sang PostResponseDto
        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt,
            VotesCount = x.VotesCount, // Sử dụng VotesCount đã tính động
            CommentsCount = x.CommentsCount,
            Score = x.Score,
            MediaUrl = x.Post.MediaUrl,
            Privacy = x.Post.Privacy,
            GroupId = x.Post.GroupId,
            CategoryId = x.Post.CategoryId,
            CategoryName = x.CategoryName,
            IsCategoryFollowed = x.IsCategoryFollowed,
            IsDeleted = x.Post.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Post.UserId.ToString("D")),
            UserVoteType =
                userVotes.ContainsKey(x.Post.Id) ? userVotes[x.Post.Id] : (VoteType?)null // Gán trạng thái vote
        }).ToList();

        return new PaginatedCursorResult<PostResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            postDtos,
            nextCursor
        );
    }

    public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts;

        // Áp dụng lọc theo FeedType
        if (request.FeedType == FeedType.Category)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }
        // FeedType == All: Không lọc theo CategoryId, lấy tất cả bài viết

        // Tách cursor thành lastScore và lastCreatedAt
        (double? lastScore, DateTime? lastCreatedAt) = ParseCursor(request.Cursor);

        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            Expression<Func<Post, bool>> filter = p =>
                GetPostScore(p, now) < lastScore.Value ||
                (GetPostScore(p, now) == lastScore.Value && p.CreatedAt < lastCreatedAt.Value);
            query = query.Where(filter);
        }

        // Tính điểm và sắp xếp, lấy thêm Category.Name
        var postsWithScore = await query
            .Select(p => new
            {
                Post = p,
                VotesCount = _dbSetUserVoteds
                    .Where(uv => uv.PostId == p.Id)
                    .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 : uv.VoteType == VoteType.DownVote ? -1 : 0),
                CommentsCount = _dbSetComments
                    .Count(c => c.PostId == p.Id && !c.IsDeleted),
                HoursSinceCreation = EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2,
                CategoryName = _dbSetCategories
                    .Where(cat => cat.Id == p.CategoryId)
                    .Select(cat => cat.Name)
                    .FirstOrDefault(),
                IsCategoryFollowed = p.CategoryId.HasValue && 
                                     _dbSetFollowCategories
                                         .Any(fc => fc.UserId == request.Id && fc.CategoryId == p.CategoryId.Value)
            })
            .Select(x => new
            {
                x.Post,
                x.VotesCount,
                x.CommentsCount,
                x.CategoryName,
                x.IsCategoryFollowed,
                Score = (x.VotesCount * 2 + x.CommentsCount) / x.HoursSinceCreation
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Post.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xử lý con trỏ (cursor) cho trang tiếp theo
        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }

        // Lấy danh sách PostId
        var postIds = postsWithScore.Select(x => x.Post.Id).ToList();

        // Lấy vote của người dùng cho các bài viết này
        var userVotes = await _dbSetUserVoteds
            .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
            .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);

        // Lấy danh sách UserId và thông tin người dùng
        var userIds = postsWithScore
            .Select(x => x.Post.UserId.ToString("D"))
            .Distinct()
            .ToList();

        var userInfos = await GetUserInfos(userIds, request.Id);

        // Chuyển đổi sang DTO
        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt,
            VotesCount = x.VotesCount,
            CommentsCount = x.CommentsCount,
            Score = x.Score,
            MediaUrl = x.Post.MediaUrl,
            Privacy = x.Post.Privacy,
            GroupId = x.Post.GroupId,
            CategoryId = x.Post.CategoryId,
            CategoryName = x.CategoryName,
            IsCategoryFollowed = x.IsCategoryFollowed,
            IsDeleted = x.Post.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Post.UserId.ToString("D")),
            UserVoteType = userVotes.ContainsKey(x.Post.Id) ? userVotes[x.Post.Id] : (VoteType?)null
        }).ToList();

        var totalCount = await query.CountAsync();

        return new PaginatedCursorResult<PostResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            postDtos,
            nextCursor
        );
    }

    // Hàm phụ xử lý cursor: chuyển đổi string cursor sang (lastScore, lastCreatedAt)
    private (double? lastScore, DateTime? lastCreatedAt) ParseCursor(string cursor)
    {
        if (string.IsNullOrEmpty(cursor))
            return (null, null);

        var parts = cursor.Split('|');
        if (parts.Length != 2 ||
            !double.TryParse(parts[0], out var parsedScore) ||
            !long.TryParse(parts[1], out var ticks))
        {
            return (null, null);
        }

        return (parsedScore, new DateTime(ticks, DateTimeKind.Utc));
    }

    // Hàm tính điểm của bài viết dựa trên số vote, số comment và khoảng thời gian
    private double? GetPostScore(Post post, DateTime now)
    {
        // Tính tổng vote: upVote tính +1, downVote tính -1
        var voteSum = _dbSetUserVoteds
            .Where(uv => uv.PostId == post.Id)
            .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 : uv.VoteType == VoteType.DownVote ? -1 : 0);

        // Điểm tính: (voteSum * 2 + số comment) / (số giờ kể từ tạo + 2)
        var timeFactor = EF.Functions.DateDiffMinute(post.CreatedAt, now) / 60.0 + 2;
        return (voteSum * 2 + post.CommentsCount) / timeFactor;
    }

    // Hàm lấy thông tin người dùng từ Redis, fallback sang gRPC nếu cần
    private async Task<Dictionary<string, UserWithInfoDto>> GetUserInfos(List<string> userIds, Guid requestUserId)
    {
        var userInfos = new Dictionary<string, UserWithInfoDto>();

        foreach (var id in userIds)
        {
            var redisKey = $"user_info:{id}";
            var userData = await _redis.HashGetAllAsync(redisKey);
            if (userData.Length > 0)
            {
                userInfos[id] = new UserWithInfoDto
                {
                    Id = Guid.TryParse(id, out var guid) ? guid : Guid.Empty,
                    Username = userData.FirstOrDefault(x => x.Name == "username").Value!,
                    Email = userData.FirstOrDefault(x => x.Name == "email").Value!,
                    IsFriend = ParseBoolFromRedis(userData.FirstOrDefault(x => x.Name == "is_friend").Value),
                    IsFollowing = ParseBoolFromRedis(userData.FirstOrDefault(x => x.Name == "is_following").Value),
                    FirstName = userData.FirstOrDefault(x => x.Name == "first_name").Value,
                    LastName = userData.FirstOrDefault(x => x.Name == "last_name").Value,
                    ProfilePictureUrl = userData.FirstOrDefault(x => x.Name == "profile_picture_url").Value,
                    Address = userData.FirstOrDefault(x => x.Name == "address").Value,
                    Gender = Enum.TryParse<Gender>(userData.FirstOrDefault(x => x.Name == "gender").Value,
                        out var gender)
                        ? gender
                        : Gender.Unknown,
                    BirthDate = DateTime.TryParse(userData.FirstOrDefault(x => x.Name == "birth_date").Value,
                        out var birthDate)
                        ? birthDate
                        : default,
                    PhoneNumber = userData.FirstOrDefault(x => x.Name == "phone_number").Value,
                };
            }
        }

        // Nếu thiếu thông tin, fallback sang gRPC
        var missingIds = userIds.Except(userInfos.Keys).ToList();
        if (missingIds.Any())
        {
            var grpcUsers = await _userClient.GetUserInfoBatchAsync(requestUserId, missingIds);
            foreach (var user in grpcUsers)
            {
                var key = user.Id.ToString("D");
                var userDto = new UserWithInfoDto
                {
                    Id = user.Id,
                    IsFriend = user.IsFriend,
                    IsFollowing = user.IsFollowing,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Bio = user.Bio,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber
                };
                userInfos[key] = userDto;
                
                // Lưu vào Redis để cache
                await _redis.HashSetAsync($"user_info:{key}", new[]
                {
                    new HashEntry("username", userDto.Username ?? string.Empty),
                    new HashEntry("email", userDto.Email ?? string.Empty),
                    new HashEntry("is_friend", userDto.IsFriend),
                    new HashEntry("is_following", userDto.IsFollowing),
                    new HashEntry("first_name", userDto.FirstName ?? string.Empty),
                    new HashEntry("last_name", userDto.LastName ?? string.Empty),
                    new HashEntry("profile_picture_url", userDto.ProfilePictureUrl ?? string.Empty),
                    new HashEntry("address", userDto.Address ?? string.Empty),
                    new HashEntry("gender", userDto.Gender.ToString()),
                    new HashEntry("birth_date", userDto.BirthDate.ToString("yyyy-MM-dd")),
                    new HashEntry("phone_number", userDto.PhoneNumber ?? string.Empty)
                });
            }
        }

        return userInfos;
    }

    private bool? ParseBoolFromRedis(RedisValue value)
    {
        return bool.TryParse(value.ToString(), out bool parsed) ? parsed : (bool?)null;
    }
}