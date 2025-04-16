using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.Group.Commands.UpdateGroup;

public class UpdateGroupHandler(IGroupRepository groupRepo, IMapper mapper) : ICommandHandler<UpdateGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
    {
       var group = mapper.Map<Domain.Models.Group>(command.Request);
       var result = await groupRepo.UpdateAsync(group);
       return new ResponseDto(
           null, result, result ? "Cập nhật nhóm thành công!" : "Cập nhật nhóm thất bại!"
       );
    }
}