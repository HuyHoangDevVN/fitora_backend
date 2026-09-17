using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.User.Responses;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Friendship.Queries.GetSuggestions;

public class GetSuggestionsHandler(
    IRepositoryBase<User> userRepo,
    IRepositoryBase<FriendShip> friendshipRepo,
    IRepositoryBase<FriendRequest> friendRequestRepo,
    IRepositoryBase<UserInfo> userInfoRepo)
    : IQueryHandler<GetSuggestionsQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = request.CurrentUserId;

        // Collect friend ids
        var friendships = await friendshipRepo.FindAsync(
            fs => fs.User1Id == currentUserId || fs.User2Id == currentUserId, cancellationToken);
        var friendIds = friendships.Select(fs =>
            fs.User1Id == currentUserId ? fs.User2Id!.Value : fs.User1Id).ToHashSet();

        // Collect pending request ids (both sent and received, status Pending)
        var pendingRequests = await friendRequestRepo.FindAsync(
            fr => (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId)
                  && fr.Status == StatusFriendRequest.Pending, cancellationToken);
        var pendingIds = new HashSet<Guid>();
        foreach (var fr in pendingRequests)
        {
            var other = fr.SenderId == currentUserId ? fr.ReceiverId!.Value : fr.SenderId;
            pendingIds.Add(other);
        }

        var excluded = new HashSet<Guid>(friendIds);
        foreach (var id in pendingIds) excluded.Add(id);
        excluded.Add(currentUserId);

        // Build predicate: user not in excluded set
        // Fetch via pagination: use GetPageAsync then map
        // To avoid loading all, filter via queryable if possible; fallback to in-memory filter with paging via SearchJoin
        // Use SearchJoin to get UserWithInfoDto and filter
        var joined = await userRepo.SearchJoinAsync<UserInfo, Guid, UserWithInfoDto>(
            u => u.Id,
            ui => ui.UserId,
            (u, ui) => new UserWithInfoDto
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                FirstName = ui.FirstName,
                LastName = ui.LastName,
                Gender = ui.Gender,
                BirthDate = ui.BirthDate ?? default,
                PhoneNumber = ui.PhoneNumber,
                Address = ui.Address,
                ProfilePictureUrl = ui.ProfilePictureUrl,
                Bio = ui.Bio,
            },
            outerSearchPredicate: excluded.Count == 0 ? null : u => !excluded.Contains(u.Id),
            innerSearchPredicate: null
        );

        var all = joined.ToList();
        var total = all.Count;
        var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var paged = all.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedResult<UserWithInfoDto>(pageIndex, pageSize, total, paged);
        return new ResponseDto(result);
    }
}
