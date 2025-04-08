using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Comments.Commands.UpdateComment;

public class UpdateCommentHandler(ICommentRepository commentRepo, IMapper mapper)
    : ICommandHandler<UpdateCommentCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = mapper.Map<Comment>(request);
        var isSuccess = await commentRepo.UpdateAsync(comment);
        return new ResponseDto(comment, isSuccess, isSuccess ? "Cập nhật thành công" : "Cập nhật thất bại");
    }
}