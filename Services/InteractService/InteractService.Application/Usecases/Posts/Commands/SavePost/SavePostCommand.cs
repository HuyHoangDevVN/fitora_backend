using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.SavePost;

public record SavePostCommand(SavePostRequest Request) : ICommand<ResponseDto>;