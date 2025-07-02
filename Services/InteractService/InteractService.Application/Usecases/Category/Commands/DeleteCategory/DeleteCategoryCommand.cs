namespace InteractService.Application.Usecases.Category.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand<bool>;