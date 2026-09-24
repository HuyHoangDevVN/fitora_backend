using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Data.Configurations;

public class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.HasIndex(x => new { x.BlockerUserId, x.BlockedUserId }).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class BlockedGroupConfiguration : IEntityTypeConfiguration<BlockedGroup>
{
    public void Configure(EntityTypeBuilder<BlockedGroup> builder)
    {
        builder.HasIndex(x => new { x.BlockerUserId, x.GroupId }).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
    }
}
