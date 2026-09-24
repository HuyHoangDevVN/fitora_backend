namespace UserService.Application.DTOs.Group.Requests;

public record SearchGroupsRequest(string? Query, int PageIndex = 0, int PageSize = 10);
