using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using UserService.Application.Data;
using UserService.Application.DTOs.Search.Responses;

namespace UserService.Application.Usecases.RecentSearch.Queries.GetRecentSearches;

public class GetRecentSearchesHandler(IApplicationDbContext db, IAuthorizeExtension auth) : IQueryHandler<GetRecentSearchesQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetRecentSearchesQuery request, CancellationToken cancellationToken)
    {
        var user = auth.GetUserFromClaimToken();
        var list = await db.RecentSearches
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.SearchedAt)
            .Select(x => new RecentSearchDto(x.Id, x.Query, x.SearchedAt))
            .ToListAsync(cancellationToken);
        return new ResponseDto(list);
    }
}
