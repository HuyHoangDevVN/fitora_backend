using InteractService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InteractService.Infrastructure.Data.Configurations;

public class ShortClipConfiguration : IEntityTypeConfiguration<ShortClip>
{
    public void Configure(EntityTypeBuilder<ShortClip> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VideoUrl).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.Caption).HasMaxLength(500);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
