using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.ApproveGroupPost;

public record ApproveGroupPostCommand(Guid PostId) : ICommand<ResponseDto>;
