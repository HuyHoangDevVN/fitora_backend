using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.Usecases.Category.Commands.UpdateCategory;

public record UpdateCategoryCommand(UpdateCategoryRequest Request) : ICommand<bool>;