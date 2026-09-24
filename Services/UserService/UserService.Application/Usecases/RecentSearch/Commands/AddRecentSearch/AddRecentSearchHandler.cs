using BuildingBlocks.DTOs;
using BuildingBlocks.Security;
using UserService.Application.Data;
using UserService.Application.DTOs.Search.Responses;

namespace UserService.Application.Usecases.RecentSearch.Commands.AddRecentSearch;

public class AddRecentSearchHandler(IApplicationDbContext db, IAuthorizeExtension auth) : ICommandHandler<AddRecentSearchCommand, ResponseDto>
{
    private const int MaxRecent = 20;

    public async Task<ResponseDto> Handle(AddRecentSearchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return new ResponseDto(null, false, "Query is required");

        var query = request.Query.Trim();
        if (query.Length > 200)
            query = query[..200];

        var user = auth.GetUserFromClaimToken();

        var existing = await db.RecentSearches
            .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Query == query, cancellationToken);

        if (existing != null)
        {
            existing.SearchedAt = DateTime.UtcNow;
            existing.LastModified = DateTime.UtcNow;
        }
        else
        {
            var entity = new Domain.Models.RecentSearch
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Query = query,
                SearchedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };
            db.RecentSearches.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);

        // cap 20 — delete oldest beyond limit
        var idsToKeep = await db.RecentSearches
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.SearchedAt)
            .Take(MaxRecent)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var toDelete = await db.RecentSearches
            .Where(x => x.UserId == user.Id && !idsToKeep.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (toDelete.Count > 0)
        {
            db.RecentSearches.RemoveRange(toDelete);
            await db.SaveChangesAsync(cancellationToken);
        }

        var all = await db.RecentSearches
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.SearchedAt)
            .Select(x => new RecentSearchDto(x.Id, x.Query, x.SearchedAt))
            .ToListAsync(cancellationToken);

        return new ResponseDto(all, Message: "Added");
    }
}
