using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Comments.Commands.DeleteComment;

public class DeleteCommentHandler(ICommentRepository commentRepo, IMapper mapper) : ICommandHandler<DeleteCommentCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var isSuccess = await commentRepo.DeleteAsync(command.Id);
        return new ResponseDto(null, isSuccess, isSuccess ? "Xóa thành công" : "Xóa thất bại");
    }
}