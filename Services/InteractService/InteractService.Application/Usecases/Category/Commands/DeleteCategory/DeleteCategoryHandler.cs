namespace InteractService.Application.Usecases.Category.Commands.DeleteCategory;

public class DeleteCategoryHandler(ICategoryRepository categoryRepo) : ICommandHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await categoryRepo.DeleteCategoryAsync(command.Id);
        return result;
    }
}