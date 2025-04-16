namespace UserService.Application.DTOs.Group.Requests;

public record GetJoinedGroupsRequest(Guid UserId, bool IsAll, int PageIndex = 0, int PageSize = 10);

public record GetJoinedGroupsFromQuery(bool IsAll, int PageIndex = 0, int PageSize = 10);