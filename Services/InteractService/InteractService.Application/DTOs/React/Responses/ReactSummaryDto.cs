namespace InteractService.Application.DTOs.React.Responses;

public class ReactSummaryDto
{
    public Guid TargetId { get; set; }
    public Dictionary<string, int> CountsByType { get; set; } = new();
    public string? MyReaction { get; set; }
}
