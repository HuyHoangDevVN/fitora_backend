using BuildingBlocks.Pagination.Base;
using UserService.Application.DTOs.Group.Requests;

namespace UserService.Application.Usecases.Group.Queries.SearchGroups;

/// <summary>
/// Privacy-aware search: public groups visible to all, private/secret only when caller is member.
/// Keyword matches Name/Description (case-insensitive contains).
/// </summary>
public record SearchGroupsQuery(SearchGroupsRequest Request, Guid CurrentUserId)
    : IQuery<PaginatedResult<Domain.Models.Group>>;
