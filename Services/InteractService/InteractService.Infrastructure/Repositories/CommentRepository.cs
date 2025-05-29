using System.Linq.Expressions;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using InteractService.Application.DTOs.Comment.Requests;
using InteractService.Application.DTOs.Comment.Responses;
using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Services.IServices;
using InteractService.Domain.Enums;
using InteractService.Infrastructure.Data;
using InteractService.Infrastructure.Grpc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace InteractService.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IRepositoryBase<Comment> _commentRepo;
    private readonly IRepositoryBase<Post> _postRepo;
    private readonly IRepositoryBase<CommentVotes> _commentVotesRepo;
    private readonly DbSet<Comment> _dbSetComments;
    private readonly DbSet<CommentVotes> _dbSetCommentVotes;
    private readonly IDatabase _redis;
    private readonly UserGrpcClient _userClient;
    private readonly ApplicationDbContext _dbContext;


    public CommentRepository(IRepositoryBase<Comment> commentRepo, IRepositoryBase<CommentVotes> commentVotesRepo,
        IRepositoryBase<Post> postRepo,
        ApplicationDbContext dbContext, IConnectionMultiplexer redis, UserGrpcClient userClient)
    {
        _commentRepo = commentRepo;
        _commentVotesRepo = commentVotesRepo;
        _postRepo = postRepo;
        _dbSetComments = dbContext.Set<Comment>();
        _dbSetCommentVotes = dbContext.Set<CommentVotes>();
        _redis = redis.GetDatabase();
        _userClient = userClient;
        _dbContext = dbContext;
    }

    public async Task<bool> CreateAsync(Comment comment)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            comment.CreatedAt = DateTime.Now;
            await _commentRepo.AddAsync(comment);
            var result = await _commentRepo.SaveChangesAsync() > 0;
            
            if (!result)
                throw new Exception("Failed to create comment");

            var post = await _postRepo.GetAsync(p => p.Id == comment.PostId)
                       ?? throw new Exception("Post not found");

            post.CommentsCount++;
            await _postRepo.UpdateAsync(p => p.Id == post.Id, post);

            var isUpdate = await _postRepo.SaveChangesAsync() > 0;

            await transaction.CommitAsync();

            return result && isUpdate;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Comment> GetByIdAsync(Guid id)
    {
        return (await _commentRepo.GetAsync(c => c.Id == id)!)!;
    }

    public async Task<bool> UpdateAsync(Comment comment)
    {
        await _commentRepo.UpdateAsync(c => c.Id == comment.Id, comment);
        return await _commentRepo.SaveChangesAsync() > 0;
    }

    public async Task<bool> VoteAsync(VoteCommentRequest request)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var comment = await _commentRepo.GetAsync(c => c.Id == request.CommentId)
                          ?? throw new Exception("Comment not found");

            var existingVote = await _commentVotesRepo
                .GetAsync(v => v.UserId == request.UserId && v.CommentId == request.CommentId);

            var voteChange = CalculateVoteChange(existingVote?.VoteType, request.VoteType);

            if (request.VoteType == VoteType.UnVote)
            {
                if (existingVote != null)
                {
                    await _commentVotesRepo.DeleteAsync(v => v.Id == existingVote.Id);
                }
            }
            else
            {
                if (existingVote != null)
                {
                    existingVote.VoteType = request.VoteType;
                    await _commentVotesRepo.UpdateAsync(v => v.Id == existingVote.Id, existingVote);
                }
                else
                {
                    var newVote = new CommentVotes
                    {
                        UserId = request.UserId,
                        CommentId = request.CommentId,
                        VoteType = request.VoteType
                    };
                    await _commentVotesRepo.AddAsync(newVote);
                }
            }

            if (voteChange != 0)
            {
                comment.VotesCont += voteChange;
                await _commentRepo.UpdateAsync(c => c.Id == comment.Id, comment);
            }

            await _commentVotesRepo.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private int CalculateVoteChange(VoteType? oldVote, VoteType newVote)
    {
        if (newVote == VoteType.UnVote)
        {
            return oldVote switch
            {
                VoteType.UpVote => -1,
                VoteType.DownVote => 1,
                _ => 0
            };
        }

        if (oldVote == null)
        {
            return newVote == VoteType.UpVote ? 1 :
                newVote == VoteType.DownVote ? -1 : 0;
        }

        return (oldVote, newVote) switch
        {
            (VoteType.UpVote, VoteType.DownVote) => -2,
            (VoteType.DownVote, VoteType.UpVote) => 2,
            _ => 0
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var comment = await _commentRepo.GetAsync(c => c.Id == id)
                          ?? throw new Exception("Comment not found");

            var post = await _postRepo.GetAsync(p => p.Id == comment.PostId)
                       ?? throw new Exception("Post not found");

            await _commentRepo.DeleteAsync(c => c.Id == id);
            
            var commentDeleted = await _commentRepo.SaveChangesAsync() > 0;
            
            if (!commentDeleted)
                throw new Exception("Failed to delete comment");
            post.CommentsCount--;
            await _postRepo.UpdateAsync(p => p.Id == post.Id, post);
            var postUpdated = await _postRepo.SaveChangesAsync() > 0;

            await transaction.CommitAsync();
            return postUpdated && commentDeleted;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new BadRequestException(e.Message);
        }
    }

    public async Task<PaginatedCursorResult<CommentResponseDto>> GetByPost(GetPostCommentsRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Comment> query = _dbSetComments
            .Where(c => c.PostId == request.PostId && !c.IsDeleted &&
                        (c.ParentCommentId == Guid.Empty || c.ParentCommentId == null)); // Chỉ lấy comment cấp 1

        // Tách cursor thành lastScore và lastCreatedAt
        (double? lastScore, DateTime? lastCreatedAt) = ParseCursor(request.Cursor);

        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            Expression<Func<Comment, bool>> filter = c =>
                GetCommentScore(c, now) < lastScore.Value ||
                (GetCommentScore(c, now) == lastScore.Value && c.CreatedAt < lastCreatedAt.Value);
            query = query.Where(filter);
        }

        // Tính điểm và sắp xếp
        var commentsWithScore = await query
            .Select(c => new
            {
                Comment = c,
                VotesCount = c.VotesCont,
                ReplyCount = _dbSetComments
                    .Count(r => r.ParentCommentId == c.Id && !r.IsDeleted),
                HoursSinceCreation = EF.Functions.DateDiffMinute(c.CreatedAt, now) / 60.0 + 2
            })
            .Select(x => new
            {
                x.Comment,
                x.VotesCount,
                x.ReplyCount,
                Score = (x.VotesCount * 2 + x.ReplyCount) /
                        x.HoursSinceCreation
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Comment.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xử lý con trỏ (cursor) cho trang tiếp theo
        string nextCursor = null;
        if (commentsWithScore.Count > request.Limit)
        {
            var lastItem = commentsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Comment.CreatedAt).Ticks}";
            commentsWithScore = commentsWithScore.Take(request.Limit).ToList();
        }

        // Lấy danh sách CommentId
        var commentIds = commentsWithScore.Select(x => x.Comment.Id).ToList();

        // Lấy vote của người dùng (giả sử UserId lấy từ context hoặc request cần có UserId)
        var userVotes = await _dbSetCommentVotes
            .Where(cv =>
                cv.UserId == request.UserId &&
                commentIds.Contains(cv.CommentId)) // Sửa: dùng commentIds thay vì postIds
            .ToDictionaryAsync(cv => cv.CommentId, cv => cv.VoteType);

        // Lấy danh sách UserId và thông tin người dùng
        var userIds = commentsWithScore
            .Select(x => x.Comment.UserId.ToString("D"))
            .Distinct()
            .ToList();

        var userInfos = await GetUserInfos(userIds, request.UserId); // Giả sử UserId từ request hoặc context

        // Chuyển đổi sang DTO
        var commentDtos = commentsWithScore.Select(x => new CommentResponseDto
        {
            Id = x.Comment.Id,
            UserId = x.Comment.UserId,
            PostId = x.Comment.PostId,
            ParentCommentId = x.Comment.ParentCommentId,
            Content = x.Comment.Content,
            MediaUrl = x.Comment.MediaUrl,
            Votes = x.VotesCount, // Sửa: dùng Votes thay vì VotesCount
            ReplyCount = x.ReplyCount,
            IsDeleted = x.Comment.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Comment.UserId.ToString("D")),
            UserVoteType = userVotes.ContainsKey(x.Comment.Id) ? userVotes[x.Comment.Id] : (VoteType?)null
        }).ToList();

        var totalCount = await query.CountAsync();

        return new PaginatedCursorResult<CommentResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            commentDtos,
            nextCursor
        );
    }

    public async Task<PaginatedCursorResult<CommentResponseDto>> GetReplies(GetCommentRepliesRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Comment> query = _dbSetComments
            .Where(c => c.ParentCommentId == request.ParentCommentId &&
                        !c.IsDeleted); // Lấy các reply của ParentCommentId

        // Tách cursor thành lastScore và lastCreatedAt
        (double? lastScore, DateTime? lastCreatedAt) = ParseCursor(request.Cursor);

        if (lastScore.HasValue && lastCreatedAt.HasValue)
        {
            Expression<Func<Comment, bool>> filter = c =>
                GetCommentScore(c, now) < lastScore.Value ||
                (GetCommentScore(c, now) == lastScore.Value && c.CreatedAt < lastCreatedAt.Value);
            query = query.Where(filter);
        }

        // Tính điểm và sắp xếp
        var commentsWithScore = await query
            .Select(c => new
            {
                Comment = c,
                VotesCount = c.VotesCont,
                ReplyCount = _dbSetComments
                    .Count(r => r.ParentCommentId == c.Id && !r.IsDeleted),
                HoursSinceCreation = EF.Functions.DateDiffMinute(c.CreatedAt, now) / 60.0 + 2
            })
            .Select(x => new
            {
                x.Comment,
                x.VotesCount,
                x.ReplyCount,
                Score = (x.VotesCount * 2 + x.ReplyCount) / x.HoursSinceCreation
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Comment.CreatedAt)
            .Take(request.Limit + 1)
            .ToListAsync();

        // Xử lý con trỏ (cursor) cho trang tiếp theo
        string nextCursor = null;
        if (commentsWithScore.Count > request.Limit)
        {
            var lastItem = commentsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Comment.CreatedAt).Ticks}";
            commentsWithScore = commentsWithScore.Take(request.Limit).ToList();
        }

        // Lấy danh sách CommentId
        var commentIds = commentsWithScore.Select(x => x.Comment.Id).ToList();

        // Lấy vote của người dùng (giả sử UserId lấy từ context hoặc request cần có UserId)
        var userVotes = await _dbSetCommentVotes
            .Where(cv => cv.UserId == request.UserId && commentIds.Contains(cv.CommentId)) // Giả sử request có UserId
            .ToDictionaryAsync(cv => cv.CommentId, cv => cv.VoteType);

        // Lấy danh sách UserId và thông tin người dùng
        var userIds = commentsWithScore
            .Select(x => x.Comment.UserId.ToString("D"))
            .Distinct()
            .ToList();

        var userInfos = await GetUserInfos(userIds, request.UserId); // Giả sử UserId từ request hoặc context

        // Chuyển đổi sang DTO
        var commentDtos = commentsWithScore.Select(x => new CommentResponseDto
        {
            Id = x.Comment.Id,
            UserId = x.Comment.UserId,
            PostId = x.Comment.PostId,
            ParentCommentId = x.Comment.ParentCommentId,
            Content = x.Comment.Content,
            MediaUrl = x.Comment.MediaUrl,
            Votes = x.VotesCount,
            ReplyCount = x.ReplyCount,
            IsDeleted = x.Comment.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Comment.UserId.ToString("D")),
            UserVoteType = userVotes.ContainsKey(x.Comment.Id) ? userVotes[x.Comment.Id] : (VoteType?)null
        }).ToList();

        var totalCount = await query.CountAsync();

        return new PaginatedCursorResult<CommentResponseDto>(
            request.Cursor,
            request.Limit,
            totalCount,
            commentDtos,
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
    private double? GetCommentScore(Comment comment, DateTime now)
    {
        // Tính tổng vote: upVote tính +1, downVote tính -1
        var voteSum = _dbSetCommentVotes
            .Where(uv => uv.CommentId == comment.Id)
            .Sum(uv => uv.VoteType == VoteType.UpVote ? 1 : uv.VoteType == VoteType.DownVote ? -1 : 0);

        // Điểm tính: (voteSum * 2 + số comment) / (số giờ kể từ tạo + 2)
        var timeFactor = EF.Functions.DateDiffMinute(comment.CreatedAt, now) / 60.0 + 2;
        return (voteSum * 2 + comment.ReplyCount) / timeFactor;
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
                    new HashEntry("is_friend", userDto.IsFriend.ToString()),
                    new HashEntry("is_following", userDto.IsFollowing.ToString()),
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