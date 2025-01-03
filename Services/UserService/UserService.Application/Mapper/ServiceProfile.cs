using AutoMapper;
using UserService.Application.Usecases.Users.Commands;
using UserService.Application.Usecases.Users.Commands.CreateUser;

namespace UserService.Application.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        #region User

        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserRegisteredMessageDto, User>().ReverseMap();
        CreateMap<CreateUserRequest, User>().ReverseMap();
        CreateMap<CreateUserRequest, CreateUserCommand>().ReverseMap();
        CreateMap<UpdateUserRequest, User>().ReverseMap();
        CreateMap<UserInfoDetail, UserInfo>().ReverseMap();
        CreateMap<UserDto, UpdateUserRequest>().ReverseMap();

        #endregion
    }
}