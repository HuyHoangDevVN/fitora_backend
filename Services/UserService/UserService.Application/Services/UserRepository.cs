using BuildingBlocks.RepositoryBase.EntityFramework;
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

}