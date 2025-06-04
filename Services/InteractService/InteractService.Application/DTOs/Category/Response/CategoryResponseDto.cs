namespace InteractService.Application.DTOs.Category.Response;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Slug { get; set; }
    public string Color { get; set; } = "#2196F3";
    public int FollowerCount { get; init; } 
    public int PostCount { get; init; }    
    public double TrendScore { get; init; } 
}