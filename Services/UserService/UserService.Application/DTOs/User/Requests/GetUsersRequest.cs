using BuildingBlocks.Pagination.Base;

namespace UserService.Application.DTOs.User.Requests;

public record GetUsersRequest(string? KeySearch, string? Username, string? Email) : PaginationRequest;