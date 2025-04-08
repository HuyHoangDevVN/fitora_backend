using BuildingBlocks.DTOs;
using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.Usecases.Category.Commands.UnfollowCategory;

public record UnfollowCategoryCommand(FollowCategoryRequest Request) : ICommand<ResponseDto>;