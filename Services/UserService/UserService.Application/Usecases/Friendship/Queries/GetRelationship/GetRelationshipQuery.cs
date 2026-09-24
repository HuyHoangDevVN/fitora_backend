using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using UserService.Application.DTOs.Friendship.Responses;

namespace UserService.Application.Usecases.Friendship.Queries.GetRelationship;

public record GetRelationshipQuery(Guid CurrentUserId, Guid TargetUserId)
    : IQuery<RelationshipDto>;
