using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.RecentSearch.Commands.DeleteRecentSearch;

public record DeleteRecentSearchCommand(Guid Id) : ICommand<ResponseDto>;
