using AuthService.Application.Auths.Commands.AssignRoles;
using AuthService.Application.Auths.Commands.AuthChangePassword;
using AuthService.Application.Auths.Commands.AuthDeleteAccount;
using AuthService.Application.Auths.Commands.AuthLockAccount;
using AuthService.Application.Auths.Commands.AuthLogin;
using AuthService.Application.Auths.Commands.AuthRegister;
using AuthService.Application.Auths.Commands.CreateKey;
using AuthService.Application.Auths.Commands.CreateRole;
using AuthService.Application.Auths.Commands.DeleteRole;
using AuthService.Application.Auths.Commands.RefreshToken;
using AuthService.Application.Auths.Commands.UpdateRole;
using AuthService.Application.DTOs.Key;
using AuthService.Application.DTOs.Key.Requests;
using AuthService.Application.DTOs.Key.Responses;
using AuthService.Application.DTOs.Roles.Requests;

namespace AuthService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        #region Authentication
        // User
        CreateMap<ApplicationUser, UserDto>().ReverseMap();
        CreateMap<UserDto, ApplicationUser>().ReverseMap();
        // Register 
        CreateMap<AuthRegisterCommand, RegisterRequestDto>().ReverseMap();
        CreateMap<RegisterRequestDto, AuthRegisterCommand>().ReverseMap();
        CreateMap<LoginResponseDto, AuthRegisterResult>().ReverseMap();
        CreateMap<AuthRegisterResult, LoginResponseDto>().ReverseMap();
        // Login
        CreateMap<AuthLoginCommand, LoginRequestDto>().ReverseMap();
        CreateMap<LoginRequestDto, AuthLoginCommand>().ReverseMap();
        CreateMap<LoginResponseDto, AuthLoginResult>().ReverseMap();
        CreateMap<AuthLoginResult, LoginResponseDto>().ReverseMap();
        // Change password
        CreateMap<AuthChangePasswordCommand, ChangePasswordRequestDto>().ReverseMap();
        // Lock account
        CreateMap<AuthLockAccountCommand, LockUserRequestDto>().ReverseMap();
        // Delete account
        CreateMap<AuthDeleteAccountCommand, DeleteUserRequestDto>().ReverseMap();
        #endregion


        #region Keys

        // Key
        CreateMap<Key, KeyDto>().ReverseMap();
        CreateMap<KeyDto, Key>().ReverseMap();
        
        // Create Key
        CreateMap<CreateKeyRequestDto, CreateKeyCommand>().ReverseMap();
        CreateMap<CreateKeyCommand, CreateKeyRequestDto>().ReverseMap();
        
        // Refresh Token Request
        CreateMap<RefreshTokenCommand, RefreshTokenByUserResponseDto>();
        CreateMap<RefreshTokenByUserResponseDto, RefreshTokenCommand>();
        // Refresh Token Response
        CreateMap<RefreshTokenResult, RefreshTokenByUserResponseDto>();
        CreateMap<RefreshTokenByUserResponseDto, RefreshTokenResult>();
        #endregion


        #region Role

        CreateMap<CreateRoleCommand, CreateRoleRequestDto>().ReverseMap();
        CreateMap<AssignRolesCommand, AssignRoleRequestDto>().ReverseMap();
        CreateMap<UpdateRoleCommand, UpdateRoleRequestDto>().ReverseMap();
        CreateMap<DeleteRoleCommand, DeleteRoleRequestDto>().ReverseMap();
        


        #endregion

    }
}