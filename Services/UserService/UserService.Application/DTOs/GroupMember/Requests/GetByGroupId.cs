namespace UserService.Application.DTOs.GroupMember.Requests;

public record GetByGroupId(Guid groupId, int pageIndex = 1, int pageSize = 10);