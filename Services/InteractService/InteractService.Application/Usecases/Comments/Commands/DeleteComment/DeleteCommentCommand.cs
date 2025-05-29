using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid Id) : ICommand<ResponseDto>;