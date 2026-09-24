using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Data.Configurations;

public class IdentityVerificationConfiguration : IEntityTypeConfiguration<IdentityVerification>
{
    public void Configure(EntityTypeBuilder<IdentityVerification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(150);
        builder.Property(x => x.DocumentType).HasMaxLength(32);
        builder.Property(x => x.DocumentNumber).HasMaxLength(64);
        builder.Property(x => x.FullName).HasMaxLength(256);
        builder.Property(x => x.FrontImageUrl).HasMaxLength(1024);
        builder.Property(x => x.BackImageUrl).HasMaxLength(1024);
        builder.HasIndex(x => x.UserId);
    }
}
