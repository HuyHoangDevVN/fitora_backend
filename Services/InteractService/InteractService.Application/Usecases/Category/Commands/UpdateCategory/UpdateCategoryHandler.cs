namespace InteractService.Application.Usecases.Category.Commands.UpdateCategory;

public class UpdateCategoryHandler(ICategoryRepository categoryRepo, IMapper mapper)
    : ICommandHandler<UpdateCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryUpdated = mapper.Map<Domain.Models.Category>(request.Request);
        var isSuccess = await categoryRepo.UpdateCategory(categoryUpdated);
        return isSuccess;
    }
}