using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.DeleteGroup;

public class DeleteGroupHandler(IGroupRepository groupRepo, IMapper mapper)
    : ICommandHandler<DeleteGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
    {
        var result = await groupRepo.DeleteAsync(command.Id);
        return new ResponseDto(
            null, result, result ? "Xóa nhóm thành công." : "Xóa nhóm thất bại."
        );
    }
}