namespace BuildingBlocks.DTOs;

public record ResponseDto(
    object? Data  = null,
    bool IsSuccess = true,
    string Message = "");
