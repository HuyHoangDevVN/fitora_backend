using AutoMapper;
using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Follow.Requests;
using UserService.Application.DTOs.Group.Requests;
using UserService.Application.DTOs.Group.Responses;
using UserService.Application.DTOs.GroupJoinRequest.Requests;
using UserService.Application.DTOs.GroupMember.Requests;
using UserService.Application.DTOs.GroupMember.Responses;
using UserService.Application.Usecases.Group.Commands.CreateGroup;
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
        CreateMap<UserInfoDto, UserInfo>().ReverseMap();

        #endregion

        #region Friendship

        

        #endregion
        
        #region Follow

        CreateMap<Follow, FollowRequest>().ReverseMap();

        #endregion

        #region Group

        CreateMap<Group, GroupDto>().ReverseMap();
        CreateMap<PaginatedResult<Group>, PaginatedResult<GroupDto>>().ReverseMap();
        CreateMap<CreateGroupRequest, Group>().ReverseMap();
        CreateMap<UpdateGroupRequest, Group>().ReverseMap();

        #endregion

        #region GroupMember

        CreateMap<GroupMember, GroupMemberDto>().ReverseMap();
        CreateMap<CreateGroupMemberRequest, GroupMember>().ReverseMap();


        #endregion

        #region GroupJoinRequest

        CreateMap<CreateGroupJoinRequest, GroupJoinRequest>().ReverseMap();

        #endregion
    }
}