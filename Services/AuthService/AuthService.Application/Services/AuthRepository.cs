using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.DTOs.Key;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Domain.Enums;
using BuildingBlocks.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services;

public class AuthRepository(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IDistributedCache cache,
    IJwtTokenGenerator jwtTokenGenerator,
    IMapper mapper,
    IKeyRepository<Guid> keyRepository,
    IHttpContextAccessor accessor,
    IRabbitMqPublisher<UserRegisteredMessageDto> rabbitMQPublisher,
    IAuthorizeExtension authorizeExtension)
    : IAuthRepository
{
    private static bool CheckKeyExpire(IEnumerable<KeyDto> keys)
    {
        if (keys?.Count() > 0)
        {
            var keyLast = keys.Last();
            if (keyLast.IsUsed == true || keyLast.IsRevoked == true || keyLast.Expire < DateTime.Now)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var checkExitUser = await userManager.FindByEmailAsync(dto.Email);
        if (checkExitUser is null)
        {
            return new LoginResponseDto(false, null, null, $"Tài khoản với email: {dto.Email} không tồn tại");
        }

        var keys = await keyRepository.GetKeysByUserIdAsync(checkExitUser.Id);
        var isPasswordValid = await userManager.CheckPasswordAsync(checkExitUser, dto.Password);
        if (!isPasswordValid)
        {
            checkExitUser.LockoutEnabled = true;
            checkExitUser.AccessFailedCount++;
            if (checkExitUser.AccessFailedCount >= 5)
            {
                checkExitUser.LockoutEnd = DateTimeOffset.Now.AddMinutes(5);
            }

            await userManager.UpdateAsync(checkExitUser);
            return new LoginResponseDto(false, null, null, "Mật khẩu không chính xác");
        }

        bool isLocked = await userManager.IsLockedOutAsync(checkExitUser);
        if (isLocked)
        {
            return new LoginResponseDto(false, null, null, $"Tài khoản với email {checkExitUser.Email} đã bị khóa");
        }

        int userStatus = checkExitUser.Status;
        if (userStatus == 3)
        {
            return new LoginResponseDto(false, null, null, "Tài khoản bị hạn chế");
        }

        var roles = await userManager.GetRolesAsync(checkExitUser);
        string accessToken = await cache.GetStringAsync($"token-{checkExitUser.Id}") ??
                             jwtTokenGenerator.GeneratorToken(checkExitUser, roles);

        await cache.SetStringAsync($"token-{checkExitUser.Id}", accessToken,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) });

        bool isCheckKey = CheckKeyExpire(keys);
        string refreshToken =
            isCheckKey ? keys.Last().Token : jwtTokenGenerator.GeneratorRefreshToken(checkExitUser.Id);

        if (!isCheckKey)
        {
            await keyRepository.CreateKeyAsync(new CreateKeyRequestDto(refreshToken, checkExitUser.Id));
        }

        var userDto = mapper.Map<UserDto>(checkExitUser);
        var token = new LoginTokenResponseDto(accessToken, refreshToken);

        return new LoginResponseDto(true, userDto, token, "Đăng nhập thành công!");
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (dto == null)
        {
            return new LoginResponseDto(false, null, null, "Dữ liệu yêu cầu không được để trống.");
        }

        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
        {
            return new LoginResponseDto(false, null, null, "Email và mật khẩu là bắt buộc.");
        }

        var checkExitUser = await userManager.FindByEmailAsync(dto.Email);
        if (checkExitUser is not null)
        {
            return new LoginResponseDto(false, null, null, $"Tài khoản với email {dto.Email} đã tồn tại.");
        }

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName,
            Status = 1,
            Avatar = ""
        };

        try
        {
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault()?.Description ?? "Không thể tạo tài khoản.";
                return new LoginResponseDto(false, null, null, error);
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtTokenGenerator.GeneratorToken(user, roles);
            var refreshToken = jwtTokenGenerator.GeneratorRefreshToken(user.Id.ToString());

            var response = new LoginResponseDto(true,
                new UserDto(user.Id.ToString(), user.UserName, user.FullName),
                new LoginTokenResponseDto(accessToken, refreshToken), "Đăng ký thành công!"
            );

            var userRegisteredMessage = new UserRegisteredMessageDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                FullName = user.FullName
            };

            await rabbitMQPublisher.PublishMessageAsync(userRegisteredMessage, "user_registration_queue");

            return response;
        }
        catch (Exception exception)
        {
            return new LoginResponseDto(false, null, null, $"Đã xảy ra lỗi: {exception.Message}");
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto)
    {
        string token = "";
        string uid = "";
        token = authorizeExtension.GetToken();

        uid = authorizeExtension.GetUserFromClaimToken().Id.ToString();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uid))
        {
            throw new BadRequestException("Invalid Token or UserId");
        }

        try
        {
            UserDto userRequest = jwtTokenGenerator.DecodeToken(token);
            if (string.IsNullOrEmpty(userRequest.Id) || !string.Equals(userRequest.Id, uid))
            {
                throw new BadRequestException("Invalid User Request");
            }

            var userFound = await userManager.FindByIdAsync(uid);
            if (userFound is null)
            {
                throw new NotFoundException("User NotFound");
            }

            var isChangePassword = await userManager.ChangePasswordAsync(userFound, dto.OldPassword, dto.NewPassword);
            if (isChangePassword.Succeeded)
            {
                return true;
            }

            foreach (var error in isChangePassword.Errors)
            {
                Console.WriteLine(error.Description);
            }

            return false;
        }
        catch (SecurityTokenException ex)
        {
            // Log the exception
            Console.WriteLine($"Token validation error: {ex.Message}");
            throw new BadRequestException("Invalid Token");
        }
    }


    public async Task<bool> LockUserAsync(LockUserRequestDto dto)
    {
        try
        {
            if (
                accessor.HttpContext!.Request.Headers.TryGetValue("x-client-id", out var uid)
                && accessor.HttpContext!.Request.Headers.TryGetValue("Authorization", out var accessToken))
            {
                if (string.IsNullOrEmpty(uid.ToString()) || string.IsNullOrEmpty(accessToken.ToString()))
                {
                    throw new BadRequestException("Token Is Null");
                }

                UserDto userToken = jwtTokenGenerator.DecodeToken(accessToken!);
                if (string.IsNullOrEmpty(userToken.Id)) throw new BadRequestException("Invalid User");
                if (!string.Equals(uid.ToString(), userToken.Id)) throw new BadRequestException("Invalid UserId");
                var userFound = await userManager.FindByIdAsync(userToken.Id) ??
                                throw new NotFoundException("User NotFound");
                userFound.Status = (int)UserStatus.Locked;
                userFound.LockoutEnabled = true;
                userFound.LockoutEnd = DateTimeOffset.Now.AddDays(dto.Expire);
                var userUpdate = await userManager.UpdateAsync(userFound);
                if (!userUpdate.Succeeded)
                {
                    return false;
                }

                return true;
            }

            throw new BadRequestException("Invalid Token");
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    public async Task<bool> LockUserByAdminAsync(Guid userId)
    {
        try
        {
            var userFound = await userManager.FindByIdAsync(userId.ToString()) ??
                            throw new NotFoundException("User NotFound");
            userFound.Status = (int)UserStatus.Locked;
            userFound.LockoutEnabled = true;
            var userUpdate = await userManager.UpdateAsync(userFound);
            if (!userUpdate.Succeeded)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    public async Task<bool> UnlockUserByAdminAsync(Guid userId)
    {
        try
        {
            var userFound = await userManager.FindByIdAsync(userId.ToString()) ??
                            throw new NotFoundException("User NotFound");
            userFound.Status = (int)UserStatus.Active;
            userFound.LockoutEnabled = false;
            var userUpdate = await userManager.UpdateAsync(userFound);
            if (!userUpdate.Succeeded)
            {
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    public async Task<bool> DeleteUserAsync(DeleteUserRequestDto dto)
    {
        try
        {
            var user = await userManager.FindByIdAsync(dto.UserId);
            if (user is null)
            {
                return false;
            }

            user.Status = (int)UserStatus.Removed;
            IdentityResult userUpdate = await userManager.UpdateAsync(user);
            if (!userUpdate.Succeeded)
            {
                throw new BadRequestException(userUpdate.Errors.FirstOrDefault()!.Description);
            }

            return true;
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
    }

    public void SetTokenInsideCookie(LoginTokenResponseDto result, HttpContext context)
    {
        context.Response.Cookies.Append("accessToken", result.AccessToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(5),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
        context.Response.Cookies.Append("refreshToken", result.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
    }
}