using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Comments.Commands.CreateComment;

public class CreateCommentHandler(ICommentRepository commentRepo, IMapper mapper)
    : ICommandHandler<CreateCommentCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = mapper.Map<Comment>(command.Request);
        var isSuccess = await commentRepo.CreateAsync(comment);
        return new ResponseDto(comment, isSuccess, isSuccess ? "Bình luận thành công" : "Bình luận thất bại");
    }
}