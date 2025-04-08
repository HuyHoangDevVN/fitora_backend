using BuildingBlocks.DTOs;

namespace InteractService.Application.Usecases.Category.Commands.CreateCategory;

public class CreateCategoryHandler(ICategoryRepository categoryRepo, IMapper mapper)
    : ICommandHandler<CreateCategoryCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = mapper.Map<Domain.Models.Category>(command.Request);
        var result = await categoryRepo.CreateCategory(category);
        return new ResponseDto(result);
    }
}