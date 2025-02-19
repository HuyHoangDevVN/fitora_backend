using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;

namespace InteractService.Application.DTOs.Post.Requests;

public record CreatePostRequest(Guid UserId, string Content, string MediaUrl, int Privacy, Guid? GroupId = null);