using InteractService.Domain.Enums;

namespace InteractService.Application.DTOs.React.Requests;

public record ToggleReactRequest(
    TargetType TargetType,
    Guid TargetId,
    ReactType ReactType);

public record ToggleReactFormBody(
    TargetType TargetType,
    Guid TargetId,
    ReactType ReactType);

public record GetReactsRequest(
    TargetType TargetType,
    Guid TargetId);
