using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.DeleteGroupPost;

public record DeleteGroupPostCommand(Guid Id) : ICommand<ResponseDto>;