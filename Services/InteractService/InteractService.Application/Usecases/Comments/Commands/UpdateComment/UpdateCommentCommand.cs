using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Comment.Requests;

namespace InteractService.Application.Usecases.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(UpdateCommentRequest Request) : ICommand<ResponseDto>;