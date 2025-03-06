using AutoMapper;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using BuildingBlocks.Pagination.Cursor;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using InteractService.Application.DTOs.Post.Requests;
using InteractService.Application.DTOs.Post.Responses;
using InteractService.Application.Services.IServices;
using InteractService.Domain.Enums;
using InteractService.Infrastructure.Data;
using InteractService.Infrastructure.Grpc;
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

    public async Task<Post> GetByIdAsync(Guid id)
    {
        return (await _postRepo.GetAsync(x => x.Id == id))!;
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetNewfeed(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSet;

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

        string nextCursor = null!;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt!).Ticks}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }

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
                    new HashEntry("birth_date",
                        userDto.BirthDate.ToString("yyyy-MM-dd")),
                    new HashEntry("phone_number", userDto.PhoneNumber ?? string.Empty)
                });
            }
        }

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
            IsDeleted = x.Post.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Post.UserId.ToString("D"))
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

    public async Task<PaginatedCursorResult<PostResponseDto>> GetPersonal(GetPostRequest request)
    {
        var now = DateTime.UtcNow;
        IQueryable<Post> query = _dbSet.Where(p => p.UserId == request.Id);

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

        string nextCursor = null;
        if (postsWithScore.Count > request.Limit)
        {
            var lastItem = postsWithScore[request.Limit];
            nextCursor = $"{lastItem.Score}|{((DateTime)lastItem.Post.CreatedAt).Ticks}";
            postsWithScore = postsWithScore.Take(request.Limit).ToList();
        }

        var totalCount = await query.CountAsync();

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
                    new HashEntry("birth_date",
                        userDto.BirthDate.ToString("yyyy-MM-dd")),
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
            VotesCount = x.Post.VotesCount,
            CommentsCount = x.Post.CommentsCount,
            Score = x.Score,
            MediaUrl = x.Post.MediaUrl,
            Privacy = x.Post.Privacy,
            GroupId = x.Post.GroupId,
            IsDeleted = x.Post.IsDeleted,
            User = userInfos.GetValueOrDefault(x.Post.UserId.ToString("D"))
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