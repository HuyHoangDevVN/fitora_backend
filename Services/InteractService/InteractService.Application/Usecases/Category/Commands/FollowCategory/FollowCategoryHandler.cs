using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Category.Commands.FollowCategory;

public class FollowCategoryHandler(ICategoryRepository categoryRepo, IMapper mapper) : ICommandHandler<FollowCategoryCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(FollowCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await categoryRepo.FollowCategoryAsync(command.Request);
        return result;
    }
}