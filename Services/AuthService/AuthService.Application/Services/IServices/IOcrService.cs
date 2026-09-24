namespace AuthService.Application.Services.IServices;

public record OcrExtractResult(
    string? DocumentNumber,
    string? FullName,
    DateTime? DateOfBirth,
    string? Address,
    string RawText
);

public interface IOcrService
{
    Task<OcrExtractResult> ExtractCccdAsync(string frontImageUrl, CancellationToken cancellationToken = default);
}
