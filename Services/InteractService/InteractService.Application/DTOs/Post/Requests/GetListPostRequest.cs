using BuildingBlocks.Pagination.Base;
using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.Post.Requests;

public record GetListPostRequest(
    Guid? CategoryId,
    Guid? UserId ,
    Guid? GroupId,
    PrivacyPost? Privacy
) : PaginationRequest;