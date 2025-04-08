using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Comment.Requests;

namespace InteractService.Application.Usecases.Comments.Commands.CreateComment;

public record CreateCommentCommand(CreateCommentRequest Request) : ICommand<ResponseDto>;