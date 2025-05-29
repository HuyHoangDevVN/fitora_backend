using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Posts.Commands.UnsavePost;

public record UnSavePostCommand(SavePostRequest Request) : ICommand<ResponseDto>;