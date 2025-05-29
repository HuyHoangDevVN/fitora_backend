using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupPost.Requests;

namespace UserService.Application.Usecases.GroupPost.Commands.CreateGroupPost;

public record CreateGroupPostCommand(CreateGroupPostRequest Request) : ICommand<ResponseDto>;