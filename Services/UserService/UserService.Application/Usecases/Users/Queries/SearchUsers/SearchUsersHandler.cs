using BuildingBlocks.DTOs;
using BuildingBlocks.Pagination.Base;
using BuildingBlocks.RepositoryBase.EntityFramework;
using UserService.Application.DTOs.User.Requests;
using UserService.Application.DTOs.User.Responses;

namespace UserService.Application.Usecases.Users.Queries.SearchUsers;

public class SearchUsersHandler(
    IRepositoryBase<User> userRepo,
    IRepositoryBase<UserInfo> userInfoRepo)
    : IQueryHandler<SearchUsersQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var q = request.Query?.Trim().ToLower() ?? string.Empty;
        var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        if (string.IsNullOrWhiteSpace(q))
        {
            var empty = new PaginatedResult<UserWithInfoDto>(pageIndex, pageSize, 0, []);
            return new ResponseDto(empty);
        }

        // Search by username / email / firstname / lastname
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
            outerSearchPredicate: null,
            innerSearchPredicate: null
        );

        var all = joined.Where(x =>
            x.Username.ToLower().Contains(q)
            || x.Email.ToLower().Contains(q)
            || (x.FirstName != null && x.FirstName.ToLower().Contains(q))
            || (x.LastName != null && x.LastName.ToLower().Contains(q))
        ).ToList();

        // De-duplicate by Id (SearchJoin may duplicate if predicate matched outer but inner filter broad)
        var distinct = all.GroupBy(x => x.Id).Select(g => g.First()).ToList();
        var total = distinct.Count;
        var paged = distinct.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        var result = new PaginatedResult<UserWithInfoDto>(pageIndex, pageSize, total, paged);
        return new ResponseDto(result);
    }
}
