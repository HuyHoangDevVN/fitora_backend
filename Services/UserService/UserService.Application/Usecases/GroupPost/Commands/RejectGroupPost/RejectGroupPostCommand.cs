using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.GroupPost.Commands.RejectGroupPost;

public record RejectGroupPostCommand(Guid PostId, string? Reason) : ICommand<ResponseDto>;
