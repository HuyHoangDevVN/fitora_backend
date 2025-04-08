using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Category.Commands.UnfollowCategory;

public class UnfollowCategoryHandler(ICategoryRepository categoryRepo, IMapper mapper) : ICommandHandler<UnfollowCategoryCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UnfollowCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await categoryRepo.UnfollowCategoryAsync(command.Request);
        return result;
    }
}