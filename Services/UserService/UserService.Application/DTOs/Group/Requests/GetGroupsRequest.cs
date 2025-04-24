namespace UserService.Application.DTOs.Group.Requests;

public record GetGroupsRequest(string Keysearch, int PageIndex = 0, int PageSize = 10);