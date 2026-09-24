using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using AuthService.Application.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace AuthService.Infrastructure.Services;

public class TesseractOcrService : IOcrService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<TesseractOcrService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<OcrExtractResult> ExtractCccdAsync(string frontImageUrl, CancellationToken cancellationToken = default)
    {
        var imageBytes = await DownloadImageAsync(frontImageUrl, cancellationToken);

        var tessDataPath = _config["Ocr:TessDataPath"] ?? "tessdata";
        var languages = _config["Ocr:Languages"] ?? "eng+vie";

        var rawText = RunTesseract(imageBytes, tessDataPath, languages);

        var documentNumber = ExtractDocumentNumber(rawText);
        var fullName = ExtractFullName(rawText);
        var dateOfBirth = ExtractDateOfBirth(rawText);
        var address = ExtractAddress(rawText);

        return new OcrExtractResult(documentNumber, fullName, dateOfBirth, address, rawText);
    }

    private async Task<byte[]> DownloadImageAsync(string relativeOrAbsoluteUrl, CancellationToken cancellationToken)
    {
        var url = relativeOrAbsoluteUrl;
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            var baseUrl = _config["Services:UploadBaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Chưa cấu hình Services:UploadBaseUrl để tải ảnh CCCD.");
            url = $"{baseUrl}{relativeOrAbsoluteUrl}";
        }

        var client = _httpClientFactory.CreateClient("OcrImageDownloader");
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private string RunTesseract(byte[] imageBytes, string tessDataPath, string languages)
    {
        try
        {
            using var engine = new TesseractEngine(tessDataPath, languages, EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            return page.GetText() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TesseractOcrService] Lỗi khi chạy OCR trên ảnh CCCD");
            return string.Empty;
        }
    }

    private static string? ExtractDocumentNumber(string text)
    {
        var match = Regex.Match(text, @"\b\d{12}\b");
        if (match.Success) return match.Value;

        match = Regex.Match(text, @"\b\d{9}\b");
        return match.Success ? match.Value : null;
    }

    private static string? ExtractFullName(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], @"(Họ và tên|Ho va ten|Full name)", RegexOptions.IgnoreCase))
            {
                var sameLine = Regex.Replace(lines[i], @".*(Họ và tên|Ho va ten|Full name)\s*[:\/]?\s*", "", RegexOptions.IgnoreCase).Trim();
                if (!string.IsNullOrWhiteSpace(sameLine) && !Regex.IsMatch(sameLine, @"^(Full name)?$", RegexOptions.IgnoreCase))
                    return CleanNameCandidate(sameLine);

                if (i + 1 < lines.Length)
                    return CleanNameCandidate(lines[i + 1]);
            }
        }
        return null;
    }

    private static string? CleanNameCandidate(string candidate)
    {
        var cleaned = candidate.Trim().TrimEnd(':', '/').Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }

    private static DateTime? ExtractDateOfBirth(string text)
    {
        var match = Regex.Match(text, @"(Ngày sinh|Ngay sinh|Date of birth)[^\d]{0,20}(\d{1,2}[\/\-.]\d{1,2}[\/\-.]\d{4})", RegexOptions.IgnoreCase);
        if (!match.Success)
            match = Regex.Match(text, @"\b(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{4})\b");

        if (!match.Success) return null;

        var dateText = match.Groups[match.Groups.Count - 1].Value;
        var normalized = Regex.Replace(dateText, @"[\-.]", "/");
        if (DateTime.TryParseExact(normalized, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
            return dob;

        return null;
    }

    private static string? ExtractAddress(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], @"(Nơi thường trú|Noi thuong tru|Place of residence|Quê quán|Que quan)", RegexOptions.IgnoreCase))
            {
                var sameLine = Regex.Replace(lines[i], @".*(Nơi thường trú|Noi thuong tru|Place of residence|Quê quán|Que quan)\s*[:\/]?\s*", "", RegexOptions.IgnoreCase).Trim();
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(sameLine)) parts.Add(sameLine);
                if (i + 1 < lines.Length && !Regex.IsMatch(lines[i + 1], @"(Có giá trị|Date of expiry|Giới tính|Sex)", RegexOptions.IgnoreCase))
                    parts.Add(lines[i + 1]);

                var joined = string.Join(", ", parts).Trim().TrimEnd(':', '/').Trim();
                return string.IsNullOrWhiteSpace(joined) ? null : joined;
            }
        }
        return null;
    }
}
