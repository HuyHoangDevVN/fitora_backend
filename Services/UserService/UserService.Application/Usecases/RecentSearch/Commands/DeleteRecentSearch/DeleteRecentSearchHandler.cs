using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using UserService.Application.Data;

namespace UserService.Application.Usecases.RecentSearch.Commands.DeleteRecentSearch;

public class DeleteRecentSearchHandler(IApplicationDbContext db, IAuthorizeExtension auth) : ICommandHandler<DeleteRecentSearchCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteRecentSearchCommand request, CancellationToken cancellationToken)
    {
        var user = auth.GetUserFromClaimToken();
        var entity = await db.RecentSearches.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == user.Id, cancellationToken);
        if (entity == null) return new ResponseDto(null, false, "Not found");
        db.RecentSearches.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return new ResponseDto(null, Message: "Deleted");
    }
}
