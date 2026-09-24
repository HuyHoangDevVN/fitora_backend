using BuildingBlocks.DTOs;
using InteractService.Application.Data;
using InteractService.Application.DTOs.React.Requests;
using InteractService.Application.DTOs.React.Responses;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Application.Usecases.React.Queries.GetReacts;

public record GetReactsQuery(GetReactsRequest Request) : IQuery<ResponseDto>;

public class GetReactsHandler(IApplicationDbContext db, BuildingBlocks.Security.IAuthorizeExtension auth)
    : IQueryHandler<GetReactsQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetReactsQuery q, CancellationToken ct)
    {
        var userId = auth.GetUserFromClaimToken().Id;
        var list = await db.Reacts
            .Where(r => r.TargetId == q.Request.TargetId && r.TargetType == q.Request.TargetType)
            .ToListAsync(ct);
        var my = list.FirstOrDefault(r => r.UserId == userId)?.ReactType.ToString();
        var summary = new ReactSummaryDto
        {
            TargetId = q.Request.TargetId,
            CountsByType = list.GroupBy(r => r.ReactType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            MyReaction = my,
        };
        return new ResponseDto(summary, IsSuccess: true);
    }
}
