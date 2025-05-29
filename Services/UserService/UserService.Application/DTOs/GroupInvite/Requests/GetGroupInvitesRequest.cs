namespace UserService.Application.DTOs.GroupInvite.Requests;

public record GetGroupInvitesRequest(Guid GroupId, int PageIndex = 1, int PageSize = 10);