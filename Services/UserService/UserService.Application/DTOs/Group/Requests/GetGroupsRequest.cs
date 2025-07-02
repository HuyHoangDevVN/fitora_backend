using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Group.Requests;

public record GetGroupsRequest(
    string? KeySearch,
    GroupPrivacy? Privacy,
    GroupStatus? Status,
    int PageIndex = 0,
    int PageSize = 10);