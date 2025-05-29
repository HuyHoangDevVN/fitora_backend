using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Comments.Commands.VoteComment;

public class VoteCommentHandler (ICommentRepository commentRepo, IMapper mapper) : ICommandHandler<VoteCommentCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(VoteCommentCommand command, CancellationToken cancellationToken)
    {
        var result = await commentRepo.VoteAsync(command.Request);
        return new ResponseDto(IsSuccess: result, Message: result ? "Vote thành công !" : "Vote thất bại", Data: null);
    }
}