using BuildingBlocks.DTOs;
using UserService.Application.DTOs.GroupPost.Requests;

namespace UserService.Application.Usecases.GroupPost.Commands.UpdateGroupPost;

public record UpdateGroupPostCommand(UpdateGroupPostRequest Request) : ICommand<ResponseDto>;