using BuildingBlocks.DTOs;

namespace UserService.Application.Usecases.RecentSearch.Commands.AddRecentSearch;

public record AddRecentSearchCommand(string Query) : ICommand<ResponseDto>;
