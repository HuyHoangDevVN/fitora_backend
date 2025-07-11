using System.Linq.Expressions;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.Friendship.Requests;
using UserService.Application.DTOs.Friendship.Responses;
using UserService.Application.Usecases.Users.Queries.GetUser;
using UserService.Domain.Enums;
using UserInfo = UserService.Domain.Models.UserInfo;

namespace UserService.Application.Services;

public class UserRepository : IUserRepository
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<UserInfo> _userInfoRepository;
    private readonly IRepositoryBase<FriendShip> _friendshipRepository;
    private readonly IRepositoryBase<Follow> _followRepository;
    private readonly IRepositoryBase<FriendRequest> _friendRequestRepository;
    private readonly IMapper _mapper;

    public UserRepository(IRepositoryBase<User> userRepository, IRepositoryBase<UserInfo> userInfoRepository,
        IRepositoryBase<FriendShip> friendshipRepository, IRepositoryBase<Follow> followRepository,
        IRepositoryBase<FriendRequest> friendRequestRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _userInfoRepository = userInfoRepository;
        _friendshipRepository = friendshipRepository;
        _followRepository = followRepository;
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }

    public async Task<bool> CreateUserAsync(User user)
    {
        await _userRepository.AddAsync(user);
        var isUserSuccess = await _userRepository.SaveChangesAsync() > 0;
        if (!isUserSuccess)
            return false;
        var userInfo = new UserInfo
        {
            UserId = user.Id,
            User = user,
            Gender = Gender.Unknown,
        };
        await _userInfoRepository.AddAsync(userInfo);
        var isUserInfoSuccess = await _userInfoRepository.SaveChangesAsync() > 0;
        return isUserInfoSuccess;
    }

    public async Task<bool> UpdateUserAsync(UserInfoDto request)
    {
        var user = _mapper.Map<UserInfo>(request);
        await _userInfoRepository.UpdateAsync(u => u.Id == request.Id, user);
        var result = (await _userInfoRepository.SaveChangesAsync() > 0);
        return result;
    }

    public async Task<User?> GetUser(GetUserRequest request)
    {
        var filters = new Dictionary<Func<GetUserRequest, bool>, Expression<Func<User, bool>>>
        {
            { r => r.Id != Guid.Empty, u => request.GetId == null ? u.Id == request.Id : u.Id == request.GetId },
            {
                r => !string.IsNullOrWhiteSpace(r.Email),
                u => request.Email != null && u.Email.ToLower() == request.Email.ToLower()
            },
            {
                r => !string.IsNullOrWhiteSpace(r.Username),
                u => request.Username != null && u.Username.ToLower() == request.Username.ToLower()
            }
        };

        foreach (var filter in filters)
        {
            if (filter.Key(request))
            {
                var user = await _userRepository.GetAsync(filter.Value);
                if (user != null)
                {
                    var userInfo = await _userInfoRepository.GetAsync(ui => ui.UserId == user.Id);
                    user.UserInfo = userInfo;
                }

                return user;
            }
        }

        return null;
    }

    public async Task<PaginatedResult<UserWithInfoDto>> GetUsers(GetUsersRequest request)
    {
        var keySearch = request.KeySearch?.ToLower() ?? string.Empty;
        var email = request.Email?.ToLower() ?? string.Empty;
        var username = request.Username?.ToLower() ?? string.Empty;

        Expression<Func<User, bool>>? searchPredicate = null;

        if (!string.IsNullOrEmpty(keySearch)
            || !string.IsNullOrEmpty(email)
            || !string.IsNullOrEmpty(username))
        {
            searchPredicate = u =>
                (string.IsNullOrEmpty(keySearch)
                 || u.Email.ToLower().Contains(keySearch)
                 || u.Username.ToLower().Contains(keySearch))
                && (string.IsNullOrEmpty(email)
                    || u.Email.ToLower().Contains(email))
                && (string.IsNullOrEmpty(username)
                    || u.Username.ToLower().Contains(username));
        }

        var users = await _userRepository.SearchJoinAsync<UserInfo, Guid, UserWithInfoDto>(
            u => u.Id,
            ui => ui.UserId,
            (u, ui) => new UserWithInfoDto
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                FirstName = ui.FirstName,
                LastName = ui.LastName,
                Gender = ui.Gender,
                BirthDate = ui.BirthDate ?? default,
                PhoneNumber = ui.PhoneNumber,
                Address = ui.Address,
                ProfilePictureUrl = ui.ProfilePictureUrl,
                Bio = ui.Bio,
            },
            searchPredicate,
            null
        );

        var count = users.Count();

        var pagedUsers = users
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<UserWithInfoDto>(request.PageIndex, request.PageSize, count, pagedUsers);
    }


    public async Task<List<UserWithInfoDto>> GetUsersByIdsAsync(Guid? id, List<Guid> userIds)
    {
        // Lấy thông tin người dùng từ cơ sở dữ liệu
        var usersWithInfo = await _userRepository.SearchJoinAsync<UserInfo, Guid, UserWithInfoDto>(
            outerKeySelector: u => u.Id,
            innerKeySelector: ui => ui.UserId,
            resultSelector: (u, ui) => new UserWithInfoDto
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                FirstName = ui.FirstName,
                LastName = ui.LastName,
                Gender = ui.Gender,
                BirthDate = ui.BirthDate ?? default(DateTime),
                PhoneNumber = ui.PhoneNumber,
                Address = ui.Address,
                ProfilePictureUrl = ui.ProfilePictureUrl,
                Bio = ui.Bio,
                IsFollowing = false, // Giá trị mặc định
                IsFriend = false // Giá trị mặc định
            },
            outerSearchPredicate: u => userIds.Contains(u.Id),
            innerSearchPredicate: null
        );

        var userList = usersWithInfo.ToList();

        // Nếu có id người dùng hiện tại, kiểm tra mối quan hệ
        if (id.HasValue)
        {
            // Lấy danh sách ID của các người dùng trong kết quả
            var targetUserIds = userList.Select(u => u.Id).ToList();

            // Lấy danh sách bạn bè của người dùng hiện tại
            var friendships = await _friendshipRepository.FindAsync(fs =>
                fs.User2Id != null && ((fs.User1Id == id.Value && targetUserIds.Contains((Guid)fs.User2Id)) ||
                                       (fs.User2Id == id.Value && targetUserIds.Contains(fs.User1Id)))
            );
            var friendIds = friendships.Select(fs => fs.User1Id == id.Value ? fs.User2Id : fs.User1Id).ToList();

            // Lấy danh sách người dùng mà người dùng hiện tại đang theo dõi
            var follows = await _followRepository.FindAsync(f =>
                f.FollowerId == id.Value && targetUserIds.Contains(f.FollowedId)
            );
            var followingIds = follows.Select(f => f.FollowedId).ToList();

            // Cập nhật giá trị IsFriend và IsFollowing cho từng người dùng
            foreach (var user in userList)
            {
                user.IsFriend = friendIds.Contains(user.Id);
                user.IsFollowing = followingIds.Contains(user.Id);
            }
        }

        return userList;
    }

    public async Task<RelationshipDto> GetRelationshipAsync(CreateFriendRequest request)
    {
        var isFriend = await _friendshipRepository.GetAsync(fs =>
            (fs.User1Id == request.senderId && fs.User2Id == request.receiverId) ||
            (fs.User2Id == request.senderId && fs.User1Id == request.receiverId)) != null;

        var isFollowing = await _followRepository.GetAsync(f =>
            f.FollowerId == request.senderId && f.FollowedId == request.receiverId) != null;

        var isFriendRequest = !isFriend && await _friendRequestRepository.GetAsync(fr =>
            fr.SenderId == request.senderId && fr.ReceiverId == request.receiverId) != null;

        return new RelationshipDto
        {
            IsFollowing = isFollowing,
            IsFriend = isFriend,
            IsFriendRequest = isFriendRequest
        };
    }
}