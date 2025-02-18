namespace BuildingBlocks.Responses;

public record UserLoginResponseBase(Guid Id, string? UserName, string? FullName);