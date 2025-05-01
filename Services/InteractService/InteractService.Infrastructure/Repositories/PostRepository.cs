using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Category.Response;
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
    private readonly IRepositoryBase<UserSaved> _userSavedRepo;
    private readonly IUserApiService _userApiService;
    private readonly DbSet<Post> _dbSetPosts;
    private readonly DbSet<UserVoted> _dbSetUserVoteds;
    private readonly DbSet<Comment> _dbSetComments;
    private readonly DbSet<Category> _dbSetCategories;
    private readonly DbSet<FollowCategory> _dbSetFollowCategories;
    private readonly IDatabase _redis;
    private readonly UserGrpcClient _userClient;

    public PostRepository(IRepositoryBase<Post> postRepo, IRepositoryBase<UserVoted> userVotedRepo,
        IRepositoryBase<UserSaved> userSavedRepo,
        IUserApiService userApiService,
        ApplicationDbContext dbContext, IConnectionMultiplexer redis, UserGrpcClient userClient)
    {
        _postRepo = postRepo;
        _userSavedRepo = userSavedRepo;
        _userApiService = userApiService;
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

    public async Task<bool> SavePostAsync(SavePostRequest request)
    {
        var post = await _postRepo.GetAsync(p => p.Id == request.PostId);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        var existingVote =
            await _userSavedRepo.GetAsync(us => us.UserId == request.UserId && us.PostId == request.PostId);

        if (existingVote != null)
        {
            throw new Exception("Post is already saved");
        }

        await _userSavedRepo.AddAsync(new UserSaved
        {
            UserId = request.UserId,
            PostId = request.PostId,
        });

        return await _userSavedRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> UnSavePostAsync(SavePostRequest request)
    {
        var post = await _postRepo.GetAsync(p => p.Id == request.PostId);
        if (post == null)
        {
            throw new Exception("Post not found");
        }

        await _userSavedRepo.DeleteAsync(us => us.UserId == request.UserId && us.PostId == request.PostId);
        return await _userSavedRepo.SaveChangesAsync() > 0;
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetSavedPosts(GetSavedPostsRequest request)
    {
        var now = DateTime.UtcNow;
        var savedPost = await _userSavedRepo.FindAsync(us => us.UserId == request.Id);
        var savedPostId = savedPost.Select(sp => sp.PostId).ToList();
        IQueryable<Post> query = _dbSetPosts.Where(p => savedPostId.Contains(p.Id));

        // Parse composite cursor nếu có
        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor!);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (_dbSetUserVoteds
                                 .Where(uv => uv.PostId == p.Id)
                                 .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 :
                                     uv.VoteType == VoteType.DownVote ? -1 : 0) * 2 +
                             _dbSetComments
                                 .Count(c => c.PostId == p.Id && !c.IsDeleted)) /
                            ((double)EF.Functions.DateDiffMinute(p.CreatedAt, now)! / 60.0 + 2)
                })
                .Where(x => x.Score < lastScore.Value ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt < lastCreatedAt.Value) ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt == lastCreatedAt.Value &&
                             x.Post.Id < lastPostId.Value))
                .Select(x => x.Post);
        }

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
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}|{lastItem.Post.Id}";
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

        // Map dữ liệu sang PostResponseDto
        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts.Where(p => p.UserId == request.Id);

        // Parse composite cursor nếu có
        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (_dbSetUserVoteds
                                 .Where(uv => uv.PostId == p.Id)
                                 .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 :
                                     uv.VoteType == VoteType.DownVote ? -1 : 0) * 2 +
                             _dbSetComments
                                 .Count(c => c.PostId == p.Id && !c.IsDeleted)) /
                            ((double)EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2)
                })
                .Where(x => x.Score < lastScore.Value ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt < lastCreatedAt.Value) ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt == lastCreatedAt.Value &&
                             x.Post.Id < lastPostId.Value))
                .Select(x => x.Post);
        }

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
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}|{lastItem.Post.Id}";
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetExploreFeed(GetExplorePostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts;

        query = query.Where(p => p.Privacy == PrivacyPost.Public);

        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (_dbSetUserVoteds
                                 .Where(uv => uv.PostId == p.Id)
                                 .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 :
                                     uv.VoteType == VoteType.DownVote ? -1 : 0) * 2 +
                             _dbSetComments
                                 .Count(c => c.PostId == p.Id && !c.IsDeleted)) /
                            ((double)EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2)
                })
                .Where(x => x.Score < lastScore.Value ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt < lastCreatedAt.Value) ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt == lastCreatedAt.Value &&
                             x.Post.Id < lastPostId.Value))
                .Select(x => x.Post);
        }

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

        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}|{lastItem.Post.Id}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }


        var postIds = postsWithScore.Select(x => x.Post.Id).ToList();
        var userVotes = await _dbSetUserVoteds
            .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
            .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);

        var userIds = postsWithScore
            .Select(x => x.Post.UserId.ToString("D"))
            .Distinct()
            .ToList();

        var userInfos = await GetUserInfos(userIds, request.Id);

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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts;
    
        var friendResponse = await _userApiService.GetFriend(String.Empty, 0, 20, CancellationToken.None);
    
        var friendIds = friendResponse.Data.Data.Select(f => f.Id).ToList();
    
        if (request.GroupId.HasValue)
        {
            query = query.Where(x => x.GroupId == request.GroupId.Value);
        }
    
        if (request.FeedType == FeedType.Category)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }
    
        query = query.Where(p =>
            p.Privacy == PrivacyPost.Public ||
            (p.Privacy == PrivacyPost.FriendsOnly && friendIds.Contains(p.UserId)) ||
            (p.Privacy != PrivacyPost.Private && p.UserId == request.Id)
        );
    
    
        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (_dbSetUserVoteds
                                 .Where(uv => uv.PostId == p.Id)
                                 .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 :
                                     uv.VoteType == VoteType.DownVote ? -1 : 0) * 2 +
                             _dbSetComments
                                 .Count(c => c.PostId == p.Id && !c.IsDeleted)) /
                            ((double)EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2)
                })
                .Where(x => x.Score < lastScore.Value ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt < lastCreatedAt.Value) ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt == lastCreatedAt.Value &&
                             x.Post.Id < lastPostId.Value))
                .Select(x => x.Post);
        }
    
    
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
    
    
        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}|{lastItem.Post.Id}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }
    
        var postIds = postsWithScore.Select(x => x.Post.Id).ToList();
        var userVotes = await _dbSetUserVoteds
            .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
            .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);
    
        var userIds = postsWithScore
            .Select(x => x.Post.UserId.ToString("D"))
            .Distinct()
            .ToList();
    
        var userInfos = await GetUserInfos(userIds, request.Id);
    
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


    // public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    // {
    //     var now = DateTime.UtcNow;
    //     var friendResponse = await _userApiService.GetFriend(String.Empty, 0, 20, CancellationToken.None);
    //     var friendIds = friendResponse.Data.Data.Select(f => f.Id).ToList();
    //     friendIds.Add(request.Id); // Đảm bảo thấy bài của chính mình
    //
    //     // Bước 1: Lấy PostId và Score
    //     var query = _dbSetPosts
    //         .Where(p => p.Privacy == PrivacyPost.Public ||
    //                     (p.Privacy == PrivacyPost.FriendsOnly && friendIds.Contains(p.UserId)) ||
    //                     (p.Privacy != PrivacyPost.Private && p.UserId == request.Id));
    //
    //     if (request.GroupId.HasValue)
    //         query = query.Where(x => x.GroupId == request.GroupId.Value);
    //     if (request.FeedType == FeedType.Category)
    //         query = query.Where(p => p.CategoryId == request.CategoryId.Value);
    //
    //     (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
    //     if (lastScore.HasValue && lastCreatedAt.HasValue && lastPostId.HasValue)
    //     {
    //         query = query
    //             .Where(p => p.Score < lastScore.Value ||
    //                         (p.Score == lastScore.Value && p.CreatedAt < lastCreatedAt.Value) ||
    //                         (p.Score == lastScore.Value && p.CreatedAt == lastCreatedAt.Value &&
    //                          p.Id < lastPostId.Value));
    //     }
    //
    //     var postsWithScore = await query
    //         .OrderByDescending(p => p.Score)
    //         .ThenByDescending(p => p.Id)
    //         .Take(request.Limit + 1)
    //         .Select(p => new
    //         {
    //             p.Id,
    //             p.Content,
    //             p.CreatedAt,
    //             p.MediaUrl,
    //             p.Privacy,
    //             p.GroupId,
    //             p.CategoryId,
    //             p.UserId,
    //             p.Score,
    //             p.IsDeleted
    //         })
    //         .ToListAsync();
    //
    //     // Tạo nextCursor
    //     string nextCursor = null;
    //     if (postsWithScore.Count > request.Limit)
    //     {
    //         var lastItem = postsWithScore[request.Limit];
    //         nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.CreatedAt).Ticks}|{lastItem.Id}";
    //         postsWithScore = postsWithScore.Take(request.Limit).ToList();
    //     }
    //
    //     var postIds = postsWithScore.Select(x => x.Id).ToList();
    //
    //     // Bước 2: Lấy chi tiết
    //     var postDetails = await _dbSetPosts
    //         .Where(p => postIds.Contains(p.Id))
    //         .GroupJoin(_dbSetUserVoteds,
    //             p => p.Id,
    //             uv => uv.PostId,
    //             (p, uvs) => new { p, Votes = uvs })
    //         .SelectMany(x => x.Votes.DefaultIfEmpty(),
    //             (p, uv) => new { p.p, VoteType = uv != null ? uv.VoteType : (VoteType?)null })
    //         .GroupBy(x => x.p.Id)
    //         .Select(g => new
    //         {
    //             PostId = g.Key,
    //             VotesCount = g.Sum(x => x.VoteType == VoteType.UpVote ? 1 : x.VoteType == VoteType.DownVote ? -1 : 0)
    //         })
    //         .Join(_dbSetComments.GroupBy(c => c.PostId)
    //                 .Select(g => new { PostId = g.Key, CommentsCount = g.Count(c => !c.IsDeleted) }),
    //             v => v.PostId,
    //             c => c.PostId,
    //             (v, c) => new
    //             {
    //                 v.PostId,
    //                 v.VotesCount,
    //                 c.CommentsCount
    //             })
    //         .ToListAsync();
    //
    //     var categoryNames = await _dbSetCategories
    //         .Where(c => postIds.Contains(c.Id))
    //         .Select(c => new { c.Id, c.Name })
    //         .ToDictionaryAsync(c => c.Id, c => c.Name);
    //
    //     var followedCategories = await _dbSetFollowCategories
    //         .Where(fc => fc.UserId == request.Id && postIds.Contains(fc.CategoryId.Value))
    //         .Select(fc => fc.CategoryId.Value)
    //         .ToListAsync();
    //
    //     // Lấy user votes và user info
    //     var userVotes = await _dbSetUserVoteds
    //         .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
    //         .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);
    //
    //     var userIds = postsWithScore.Select(x => x.UserId.ToString("D")).Distinct().ToList();
    //     var userInfos = await GetUserInfos(userIds, request.Id);
    //
    //     // Tạo DTO
    //     var postDtos = postsWithScore.Select(x =>
    //     {
    //         var details = postDetails.FirstOrDefault(d => d.PostId == x.Id);
    //         return new PostResponseDto
    //         {
    //             Id = x.Id,
    //             Content = x.Content,
    //             CreatedAt = (DateTimeOffset)x.CreatedAt,
    //             VotesCount = details?.VotesCount ?? 0,
    //             CommentsCount = details?.CommentsCount ?? 0,
    //             Score = x.Score,
    //             MediaUrl = x.MediaUrl,
    //             Privacy = x.Privacy,
    //             GroupId = x.GroupId,
    //             CategoryId = x.CategoryId,
    //             CategoryName = categoryNames.GetValueOrDefault(x.CategoryId ?? Guid.Empty),
    //             IsCategoryFollowed = x.CategoryId.HasValue && followedCategories.Contains(x.CategoryId.Value),
    //             IsDeleted = x.IsDeleted,
    //             User = userInfos.GetValueOrDefault(x.UserId.ToString("D")),
    //             UserVoteType = userVotes.ContainsKey(x.Id) ? userVotes[x.Id] : (VoteType?)null
    //         };
    //     }).ToList();
    //
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetTrendingFeed(GetTrendingPostRequest request,
        IEnumerable<CategoryResponseDto> trendingCategories)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts;

        // Lấy danh sách danh mục thịnh hành
        var trendingCategoryIds = trendingCategories.Select(c => c.Id).ToList();

        // Lọc bài viết thuộc các danh mục thịnh hành
        query = query.Where(p => p.CategoryId.HasValue && trendingCategoryIds.Contains(p.CategoryId.Value));

        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (_dbSetUserVoteds
                                 .Where(uv => uv.PostId == p.Id)
                                 .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 :
                                     uv.VoteType == VoteType.DownVote ? -1 : 0) * 2 +
                             _dbSetComments
                                 .Count(c => c.PostId == p.Id && !c.IsDeleted)) /
                            ((double)EF.Functions.DateDiffMinute(p.CreatedAt, now) / 60.0 + 2)
                })
                .Where(x => x.Score < lastScore.Value ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt < lastCreatedAt.Value) ||
                            (x.Score == lastScore.Value && x.Post.CreatedAt == lastCreatedAt.Value &&
                             x.Post.Id < lastPostId.Value))
                .Select(x => x.Post);
        }

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

        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}|{lastItem.Post.Id}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }


        var postIds = postsWithScore.Select(x => x.Post.Id).ToList();
        var userVotes = await _dbSetUserVoteds
            .Where(uv => uv.UserId == request.Id && postIds.Contains(uv.PostId))
            .ToDictionaryAsync(uv => uv.PostId, uv => uv.VoteType);

        var userIds = postsWithScore
            .Select(x => x.Post.UserId.ToString("D"))
            .Distinct()
            .ToList();

        var userInfos = await GetUserInfos(userIds, request.Id);

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
    private (double? score, DateTime? createdAt, Guid? postId) ParseCursor(string cursor)
    {
        if (string.IsNullOrEmpty(cursor)) return (null, null, null);
        var parts = cursor.Split('|');
        if (parts.Length != 3) return (null, null, null);
        if (double.TryParse(parts[0], out var score) &&
            long.TryParse(parts[1], out var ticks) &&
            Guid.TryParse(parts[2], out var postId))
        {
            return (score, new DateTime(ticks), postId);
        }

        return (null, null, null);
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