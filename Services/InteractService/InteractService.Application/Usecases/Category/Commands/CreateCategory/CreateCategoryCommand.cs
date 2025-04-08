using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.Usecases.Category.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Request) : ICommand<ResponseDto>;