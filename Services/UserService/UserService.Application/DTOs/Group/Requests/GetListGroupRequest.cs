using BuildingBlocks.Pagination.Base;
using UserService.Domain.Enums;

namespace UserService.Application.DTOs.Group.Requests;

public record GetListGroupRequest(string? Name, GroupPrivacy? Privacy, GroupStatus? Status) : PaginationRequest;