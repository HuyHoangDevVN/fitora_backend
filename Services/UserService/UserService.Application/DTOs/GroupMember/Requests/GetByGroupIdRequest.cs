namespace UserService.Application.DTOs.GroupMember.Requests;

public record GetByGroupIdRequest(Guid GroupId, int PageIndex = 0, int PageSize = 10);