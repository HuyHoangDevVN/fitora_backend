namespace UserService.Application.DTOs.Group.Requests;

public record GetManagedGroupsRequest(Guid UserId, int PageIndex = 0, int PageSize = 10);

public record GetManagedGroupsFromQuery(int PageIndex = 0, int PageSize = 10);