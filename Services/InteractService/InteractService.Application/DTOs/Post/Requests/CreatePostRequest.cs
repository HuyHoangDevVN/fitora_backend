using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;
using InteractService.Application.DTOs.Category.Requests;

namespace InteractService.Application.DTOs.Post.Requests;

public record CreatePostRequest(
    Guid UserId,
    string Content,
    string MediaUrl,
    int Privacy,
    Guid? GroupId,
    Guid? CategoryId);
    
public record CreatePostFromBody(
    string Content,
    string MediaUrl,
    int Privacy,
    Guid? GroupId,
    Guid? CategoryId);
    