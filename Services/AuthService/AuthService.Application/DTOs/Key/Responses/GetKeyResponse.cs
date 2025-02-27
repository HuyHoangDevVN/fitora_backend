namespace AuthService.Application.DTOs.Key.Responses;

public record GetKeysResponse(object MetaData, bool IsSuccess = true, string Message = "");
