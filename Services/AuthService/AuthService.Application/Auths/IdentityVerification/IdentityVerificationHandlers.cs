using AuthService.Application.Services.IServices;
using BuildingBlocks.Security;

namespace AuthService.Application.Auths.IdentityVerification;

public record UploadDocumentCommand(string FrontImageUrl, string? BackImageUrl) : ICommand<UploadDocumentResult>;
public record UploadDocumentResult(Guid Id, string Message);
public record OcrDocumentCommand(Guid VerificationId) : ICommand<OcrDocumentResult>;
public record OcrDocumentResult(string? DocumentNumber, string? FullName, DateTime? DateOfBirth, string? Address, string Message);
public record SubmitVerificationCommand(Guid VerificationId, string DocumentNumber, string FullName, DateTime DateOfBirth, string Address) : ICommand<SubmitVerificationResult>;
public record SubmitVerificationResult(bool IsSuccess, string Message);
public record GetVerificationStatusQuery : IQuery<GetVerificationStatusResult>;
public record GetVerificationStatusResult(Guid? Id, string Status, string? RejectionReason);

public class UploadDocumentHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<UploadDocumentCommand, UploadDocumentResult>
{
    public async Task<UploadDocumentResult> Handle(UploadDocumentCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var v = new Domain.Models.IdentityVerification
        {
            Id = Guid.NewGuid(), UserId = userId, FrontImageUrl = cmd.FrontImageUrl,
            BackImageUrl = cmd.BackImageUrl, Status = Domain.Models.VerificationStatus.Pending, CreatedAt = DateTime.UtcNow
        };
        db.IdentityVerifications.Add(v);
        await db.SaveChangesAsync(ct);
        return new(v.Id, "Uploaded");
    }
}

public class OcrDocumentHandler(IApplicationDbContext db, IAuthorizeExtension auth, IOcrService ocrService)
    : ICommandHandler<OcrDocumentCommand, OcrDocumentResult>
{
    public async Task<OcrDocumentResult> Handle(OcrDocumentCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var v = await db.IdentityVerifications.FirstOrDefaultAsync(x => x.Id == cmd.VerificationId && x.UserId == userId, ct)
            ?? throw new BuildingBlocks.Exceptions.NotFoundException("Verification not found");

        if (string.IsNullOrEmpty(v.FrontImageUrl))
            throw new BuildingBlocks.Exceptions.BadRequestException("Chưa có ảnh mặt trước CCCD để quét OCR");

        var extracted = await ocrService.ExtractCccdAsync(v.FrontImageUrl, ct);

        v.DocumentNumber = extracted.DocumentNumber ?? v.DocumentNumber;
        v.FullName = extracted.FullName ?? v.FullName;
        v.DateOfBirth = extracted.DateOfBirth ?? v.DateOfBirth;
        v.Address = extracted.Address ?? v.Address;
        await db.SaveChangesAsync(ct);

        var message = string.IsNullOrWhiteSpace(extracted.RawText)
            ? "Không đọc được nội dung từ ảnh. Vui lòng chụp rõ hơn hoặc nhập tay."
            : "Đã quét OCR (Tesseract) - vui lòng kiểm tra lại thông tin trước khi gửi";

        return new(v.DocumentNumber, v.FullName, v.DateOfBirth, v.Address, message);
    }
}

public class SubmitVerificationHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : ICommandHandler<SubmitVerificationCommand, SubmitVerificationResult>
{
    public async Task<SubmitVerificationResult> Handle(SubmitVerificationCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var v = await db.IdentityVerifications.FirstOrDefaultAsync(x => x.Id == cmd.VerificationId && x.UserId == userId, ct);
        if (v == null) return new(false, "Verification not found");
        v.DocumentNumber = cmd.DocumentNumber; v.FullName = cmd.FullName;
        v.DateOfBirth = cmd.DateOfBirth; v.Address = cmd.Address;
        v.Status = Domain.Models.VerificationStatus.Pending;
        await db.SaveChangesAsync(ct);
        return new(true, "Submitted for review");
    }
}

public class GetVerificationStatusHandler(IApplicationDbContext db, IAuthorizeExtension auth)
    : IQueryHandler<GetVerificationStatusQuery, GetVerificationStatusResult>
{
    public async Task<GetVerificationStatusResult> Handle(GetVerificationStatusQuery q, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id.ToString();
        var v = await db.IdentityVerifications.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (v == null) return new(null, "None", null);
        return new(v.Id, v.Status.ToString(), v.RejectionReason);
    }
}
