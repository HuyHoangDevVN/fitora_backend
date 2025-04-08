using BuildingBlocks.Abstractions;
using BuildingBlocks.Abstractions.Entities;

namespace InteractService.Domain.Models;

public class Category : Entity<Guid>, ISoftDelete
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    
    public ICollection<Post> Posts { get; set; }

    public Guid? ParentId { get; set; }
    public Category Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; }

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}