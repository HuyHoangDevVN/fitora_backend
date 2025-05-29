using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Comment.Requests;

namespace InteractService.Application.Usecases.Comments.Commands.VoteComment;

public record VoteCommentCommand(VoteCommentRequest Request) : ICommand<ResponseDto>;