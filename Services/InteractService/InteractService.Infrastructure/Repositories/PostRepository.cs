using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IRepositoryBase<UserVoted> _userVotedRepo;
    private readonly IRepositoryBase<UserSaved> _userSavedRepo;
    private readonly IUserApiService _userApiService;
    private readonly DbSet<Post> _dbSetPosts;
    private readonly DbSet<UserVoted> _dbSetUserVoteds;
    private readonly DbSet<Category> _dbSetCategories;
    private readonly DbSet<FollowCategory> _dbSetFollowCategories;
    private readonly IDatabase _redis;
    private readonly UserGrpcClient _userClient;
    private readonly IAuthorizeExtension _authorizeExtension;
    private readonly IElasticsearchPostService _elasticsearchPostService;

    public PostRepository(IRepositoryBase<Post> postRepo, IRepositoryBase<UserVoted> userVotedRepo,
        IRepositoryBase<UserSaved> userSavedRepo,
        IUserApiService userApiService,
        ApplicationDbContext dbContext, IConnectionMultiplexer redis, UserGrpcClient userClient,
        IAuthorizeExtension authorizeExtension,
        IElasticsearchPostService elasticsearchPostService)
    {
        _postRepo = postRepo;
        _userSavedRepo = userSavedRepo;
        _userVotedRepo = userVotedRepo;
        _dbSetPosts = dbContext.Set<Post>();
        _dbSetUserVoteds = dbContext.Set<UserVoted>();
        _dbSetCategories = dbContext.Set<Category>();
        _dbSetFollowCategories = dbContext.Set<FollowCategory>();
        _redis = redis.GetDatabase();
        _userApiService = userApiService;
        _userClient = userClient;
        _dbContext = dbContext;
        _authorizeExtension = authorizeExtension;
        _elasticsearchPostService = elasticsearchPostService;
    }

    public async Task<bool> CreateAsync(Post post)
    {
        post.CreatedAt = DateTime.Now;
        post.CreatedBy = post.UserId;
        await _postRepo.AddAsync(post);
        var saved = await _postRepo.SaveChangesAsync() > 0;
        if (saved)
        {
            await _elasticsearchPostService.IndexPostAsync(post);
        }
        return saved;
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        return (await _postRepo.GetAsync(x => x.Id == id))!;
    }

    public async Task<bool> UpdateAsync(Post post)
    {
        await _postRepo.UpdateAsync(p => p.Id == post.Id, post);
        var saved = await _postRepo.SaveChangesAsync() > 0;
        if (saved)
        {
            await _elasticsearchPostService.UpdatePostAsync(post);
        }
        return saved;
    }

    public async Task<bool> VoteAsync(VotePostRequest request)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var post = await _postRepo.GetAsync(p => p.Id == request.PostId)
                       ?? throw new Exception("Post not found");

            var userVote = await _userVotedRepo.GetAsync(uv =>
                uv.UserId == request.UserId && uv.PostId == request.PostId);

            switch (request.VoteType)
            {
                case VoteType.UnVote:
                    await HandleUnVoteAsync(userVote, post);
                    break;

                case VoteType.UpVote:
                case VoteType.DownVote:
                    if (userVote == null)
                        await HandleNewVoteAsync(request, post);
                    else if (userVote.VoteType != request.VoteType)
                        await HandleChangeVoteAsync(userVote, request.VoteType, post);
                    break;
            }

            await _userVotedRepo.SaveChangesAsync();
            await _postRepo.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task HandleUnVoteAsync(UserVoted? existingVote, Post post)
    {
        if (existingVote == null) return;

        UpdateVotesCountOnUnVote(existingVote.VoteType, post);
        await _userVotedRepo.DeleteAsync(uv => uv.Id == existingVote.Id);
    }

    private async Task HandleNewVoteAsync(VotePostRequest request, Post post)
    {
        await _userVotedRepo.AddAsync(new UserVoted
        {
            UserId = request.UserId,
            PostId = request.PostId,
            VoteType = request.VoteType
        });

        UpdateVoteCountOnNewVote(request.VoteType, post);
    }

    private async Task HandleChangeVoteAsync(UserVoted existingVote, VoteType newVoteType, Post post)
    {
        UpdateVotesCountOnChange(existingVote.VoteType, newVoteType, post);
        existingVote.VoteType = newVoteType;
        await _userVotedRepo.UpdateAsync(uv => uv.Id == existingVote.Id, existingVote);
    }

    private void UpdateVoteCountOnNewVote(VoteType voteType, Post post)
    {
        if (voteType == VoteType.UpVote)
            post.VotesCount += 1;
        else if (voteType == VoteType.DownVote)
            post.VotesCount -= 1;
    }

    private void UpdateVotesCountOnUnVote(VoteType oldVote, Post post)
    {
        if (oldVote == VoteType.UpVote)
            post.VotesCount = Math.Max(0, post.VotesCount - 1);
        else if (oldVote == VoteType.DownVote)
            post.VotesCount += 1;
    }

    private void UpdateVotesCountOnChange(VoteType oldVote, VoteType newVote, Post post)
    {
        if (oldVote == VoteType.UpVote && newVote == VoteType.DownVote)
            post.VotesCount -= 2;
        else if (oldVote == VoteType.DownVote && newVote == VoteType.UpVote)
            post.VotesCount += 2;
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
            var post = await _postRepo.GetAsync(p => p.Id == id);
            if (post == null) return false;
            post.IsDeleted = true;
            await _postRepo.UpdateAsync(p => p.Id == id, post);
            var saved = await _postRepo.SaveChangesAsync() > 0;
            if (saved)
            {
                await _elasticsearchPostService.DeletePostAsync(id);
            }
            return saved;
        }
        catch (Exception e)
        {
            throw new Exception($"Delete failed: {e.Message}");
        }
    }

    public async Task<PaginatedCursorResult<PostResponseDto>> GetSavedPosts(GetSavedPostsRequest request)
    {
        var now = DateTime.UtcNow;
        var savedPost = await _userSavedRepo.FindAsync(us => us.UserId == request.Id);
        var savedPostId = savedPost.Select(sp => sp.PostId).ToList();
        IQueryable<Post> query = _dbSetPosts.Where(p => savedPostId.Contains(p.Id));

        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor!);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (p.VotesCount +
                             p.CommentsCount) /
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
                p.VotesCount,
                p.CommentsCount,
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
        var userInfos = await GetUserInfos(userIds, request.Id);

        // Map dữ liệu sang PostResponseDto
        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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
            UserVoteType =
                userVotes.ContainsKey(x.Post.Id) ? userVotes[x.Post.Id] : (VoteType?)null
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
        var privacy = _authorizeExtension.GetUserFromClaimToken().Id == request.Id ? 0 : request.IsFriend == true ? 1 : 2;

        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts.Where(p => p.Privacy >= (PrivacyPost)privacy && p.UserId == request.Id);

        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (p.VotesCount * 2 +
                             p.CommentsCount) /
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
                p.VotesCount,
                p.CommentsCount,
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

        var totalCount = await query.CountAsync();

        var userIds = postsWithScore.Select(x => x.Post.UserId.ToString("D")).Distinct().ToList();

        var userInfos = await GetUserInfos(userIds, request.Id);

        var postDtos = postsWithScore.Select(x => new PostResponseDto
        {
            Id = x.Post.Id,
            Content = x.Post.Content,
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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
            UserVoteType =
                userVotes.ContainsKey(x.Post.Id) ? userVotes[x.Post.Id] : (VoteType?)null
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
                    Score = (p.VotesCount * 2 +
                             p.CommentsCount) /
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
                p.VotesCount,
                p.CommentsCount,
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
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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

    public async Task<PaginatedResult<PostResponseDto>> GetListPost(GetListPostRequest request)
    {
        var posts = await _postRepo.GetPageAsync(request, CancellationToken.None, p =>
            (!request.UserId.HasValue || p.UserId == request.UserId) &&
            (!request.GroupId.HasValue || p.GroupId == request.GroupId) &&
            (!request.CategoryId.HasValue || p.CategoryId == request.CategoryId) &&
            !p.IsDeleted);
        var totalCount = posts.Data.Count();
        var postDto = posts.Data.Select(p => new PostResponseDto
        {
            Id = p.Id,
            Content = p.Content,
            CreatedAt = (DateTimeOffset)p.CreatedAt!,
            VotesCount = p.VotesCount,
            CommentsCount = p.CommentsCount,
            Score = p.Score,
            MediaUrl = p.MediaUrl,
            Privacy = p.Privacy,
            GroupId = p.GroupId,
            CategoryId = p.CategoryId,
            IsDeleted = p.IsDeleted
        }).ToList();
        return new PaginatedResult<PostResponseDto>(
            posts.PageIndex,
            posts.PageSize,
            totalCount,
            postDto
        );
    }

    public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    {
        if (!string.IsNullOrEmpty(request.KeySearch))
        {
            var esPosts = await _elasticsearchPostService.SearchByContentAsync(request.KeySearch, request.Limit);
            var esPostIds = esPosts.Select(p => p.Id).ToList();
            var posts = await _dbSetPosts.Where(p => esPostIds.Contains(p.Id) && !p.IsDeleted).ToListAsync();
            var esUserIds = posts.Select(x => x.UserId.ToString("D")).Distinct().ToList();
            var esUserInfos = await GetUserInfos(esUserIds, request.Id);
            var esPostDtos = posts.Select(p => new PostResponseDto
            {
                Id = p.Id,
                Content = p.Content,
                CreatedAt = (DateTimeOffset)p.CreatedAt!,
                VotesCount = p.VotesCount,
                CommentsCount = p.CommentsCount,
                Score = null, // Không tính score khi search
                MediaUrl = p.MediaUrl,
                Privacy = p.Privacy,
                GroupId = p.GroupId,
                CategoryId = p.CategoryId,
                CategoryName = _dbSetCategories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name,
                IsCategoryFollowed = p.CategoryId.HasValue && _dbSetFollowCategories.Any(fc => fc.UserId == request.Id && fc.CategoryId == p.CategoryId.Value),
                IsDeleted = p.IsDeleted,
                User = esUserInfos.GetValueOrDefault(p.UserId.ToString("D")),
                UserVoteType = _dbSetUserVoteds.FirstOrDefault(uv => uv.UserId == request.Id && uv.PostId == p.Id)?.VoteType
            }).ToList();
            return new PaginatedCursorResult<PostResponseDto>(
                request.Cursor,
                request.Limit,
                esPostDtos.Count,
                esPostDtos,
                null // Không phân trang khi search
            );
        }

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
                    Score = (p.VotesCount * 2 +
                             p.CommentsCount) /
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
                p.VotesCount,
                p.CommentsCount,
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
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetTrendingFeed(GetTrendingPostRequest request,
        IEnumerable<CategoryResponseDto> trendingCategories)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSetPosts;

        var trendingCategoryIds = trendingCategories.Select(c => c.Id).ToList();

        query = query.Where(p => p.CategoryId.HasValue && trendingCategoryIds.Contains(p.CategoryId.Value) && p.Privacy >= PrivacyPost.FriendsOnly);

        (double? lastScore, DateTime? lastCreatedAt, Guid? lastPostId) = ParseCursor(request.Cursor);
        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            query = query
                .Select(p => new
                {
                    Post = p,
                    Score = (p.VotesCount * 2 +
                             p.CommentsCount) /
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
                p.VotesCount,
                p.CommentsCount,
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
            CreatedAt = (DateTimeOffset)x.Post.CreatedAt!,
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

    /// Đồng bộ toàn bộ dữ liệu Post từ database sang Elasticsearch, chia batch nếu quá lớn
    public async Task<bool> SyncAllPostsToElasticsearchAsync(int batchSize = 1000)
    {
        var totalPosts = await _dbSetPosts.CountAsync();
        bool success = true;
        for (int i = 0; i < totalPosts; i += batchSize)
        {
            var batch = await _dbSetPosts.OrderBy(p => p.Id).Skip(i).Take(batchSize).ToListAsync();
            if (batch.Count > 0)
            {
                await _elasticsearchPostService.BulkIndexPostsAsync(batch);
            }
        }
        return success;
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