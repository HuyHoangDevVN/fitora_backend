using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.RecentSearch.Commands.ClearRecentSearches;

public record ClearRecentSearchesCommand : ICommand<ResponseDto>;
