using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.User.Responses;

namespace UserService.Application.Usecases.Friendship.Queries.GetSuggestions;

public record GetSuggestionsQuery(Guid CurrentUserId, int PageIndex = 1, int PageSize = 10)
    : IQuery<ResponseDto>;
