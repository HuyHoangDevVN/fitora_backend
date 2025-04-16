namespace UserService.Application.DTOs.GroupInvite.Requests;

public record GetSentGroupInviteRequest(Guid Id, int PageIndex = 0, int PageSize = 10);