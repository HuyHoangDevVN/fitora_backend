using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using InteractService.Application.Data;
using InteractService.Application.DTOs.React.Requests;
using InteractService.Application.DTOs.React.Responses;
using InteractService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.React.Commands.ToggleReact;

public class ToggleReactHandler(IApplicationDbContext db, BuildingBlocks.Security.IAuthorizeExtension auth)
    : ICommandHandler<ToggleReactCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ToggleReactCommand cmd, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var req = cmd.Request;
        // Mỗi user tối đa 1 reaction/post — chuyển loại thì update, bấm lại thì xóa (idempotent toggle)
        var existing = await db.Reacts.FirstOrDefaultAsync(
            r => r.UserId == userId && r.TargetId == req.TargetId && r.TargetType == req.TargetType, ct);

        if (existing is not null)
        {
            if (existing.ReactType == req.ReactType)
            {
                db.Reacts.Remove(existing);
                await db.SaveChangesAsync(ct);
                return new ResponseDto(await Summary(req, null, ct), IsSuccess: true, Message: "Đã gỡ cảm xúc");
            }
            existing.ReactType = req.ReactType;
        }
        else
        {
            var react = new Domain.Models.React
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TargetType = req.TargetType,
                TargetId = req.TargetId,
                ReactType = req.ReactType,
            };
            await db.Reacts.AddAsync(react, ct);
        }
        await db.SaveChangesAsync(ct);
        return new ResponseDto(await Summary(req, req.ReactType.ToString(), ct), IsSuccess: true, Message: "Đã thả cảm xúc");
    }

    private async Task<ReactSummaryDto> Summary(ToggleReactRequest req, string? my, CancellationToken ct)
    {
        var list = await db.Reacts
            .Where(r => r.TargetId == req.TargetId && r.TargetType == req.TargetType)
            .ToListAsync(ct);
        return new ReactSummaryDto
        {
            TargetId = req.TargetId,
            CountsByType = list.GroupBy(r => r.ReactType.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            MyReaction = my,
        };
    }
}
