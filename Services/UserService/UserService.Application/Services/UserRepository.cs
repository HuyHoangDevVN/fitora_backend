using System.Linq.Expressions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.Usecases.Users.Queries.GetUser;
using UserService.Domain.Enums;
using UserInfo = UserService.Domain.Models.UserInfo;

namespace UserService.Application.Services;

public class UserRepository : IUserRepository
{
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<UserInfo> _userInfoRepository;
    private readonly IMapper _mapper;

    public UserRepository(IRepositoryBase<User> userRepository, IRepositoryBase<UserInfo> userInfoRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _userInfoRepository = userInfoRepository;
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

    public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
    {
        var user = _mapper.Map<User>(request);

        await _userRepository.UpdateAsync(u => u.Id == request.Id, user);
        if (await _userRepository.SaveChangesAsync() <= 0)
            return false;

        if (request.UserInfo != null)
        {
            var userInfo = _mapper.Map<UserInfo>(request.UserInfo);
            await _userInfoRepository.UpdateAsync(u => u.Id == request.Id, userInfo);
            if (await _userInfoRepository.SaveChangesAsync() <= 0)
                return false;
        }

        return true;
    }

    public async Task<User?> GetUser(GetUserRequest request)
    {
        var filters = new Dictionary<Func<GetUserRequest, bool>, Expression<Func<User, bool>>>
        {
            { r => r.Id != Guid.Empty, u => u.Id == request.Id },
            {
                r => !string.IsNullOrWhiteSpace(r.Email),
                u => request.Email != null && u.Email.ToLower() == request.Email.ToLower()
            },
            {
                r => !string.IsNullOrWhiteSpace(r.UserName),
                u => request.UserName != null && u.Username.ToLower() == request.UserName.ToLower()
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
                BirthDate = ui.BirthDate ?? default(DateTime),
                PhoneNumber = ui.PhoneNumber,
                Address = ui.Address,
                ProfilePictureUrl = ui.ProfilePictureUrl,
                Bio = ui.Bio,
            },
            u => u.Email.Contains(request.Email!.ToLower()) || u.Username.Contains(request.Email!.ToLower()),
            null
        );
        var count = users.Count();
        return new PaginatedResult<UserWithInfoDto>(request.PageIndex, request.PageSize, count, users);
    }
}