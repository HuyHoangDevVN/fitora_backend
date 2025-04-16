namespace UserService.Application.DTOs.GroupInvite.Requests;

public record GetReceivedGroupInviteRequest(Guid Id, int PageIndex = 0, int PageSize = 10);