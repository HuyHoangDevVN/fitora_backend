using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using UserService.Application.Data;

namespace UserService.Application.Usecases.RecentSearch.Commands.ClearRecentSearches;

public class ClearRecentSearchesHandler(IApplicationDbContext db, IAuthorizeExtension auth) : ICommandHandler<ClearRecentSearchesCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ClearRecentSearchesCommand request, CancellationToken cancellationToken)
    {
        var user = auth.GetUserFromClaimToken();
        var list = await db.RecentSearches.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);
        if (list.Count > 0)
        {
            db.RecentSearches.RemoveRange(list);
            await db.SaveChangesAsync(cancellationToken);
        }
        return new ResponseDto(null, Message: "Cleared");
    }
}
