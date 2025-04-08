using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.Usecases.Category.Commands.FollowCategory;

public record FollowCategoryCommand(FollowCategoryRequest Request): ICommand<ResponseDto>;