using InteractService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InteractService.Infrastructure.Data.Configurations;

public class StudyPostConfiguration : IEntityTypeConfiguration<StudyPost>
{
    public void Configure(EntityTypeBuilder<StudyPost> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Content).IsRequired();
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.CategoryId);
    }
}
