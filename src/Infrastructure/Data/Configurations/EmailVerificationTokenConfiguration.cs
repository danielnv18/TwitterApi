using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterCloneApi.Domain.Entities;

namespace TwitterCloneApi.Infrastructure.Data.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens");

        builder.HasKey(evt => evt.Id);

        builder.Property(evt => evt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(evt => evt.UserId)
            .IsRequired();

        builder.Property(evt => evt.Token)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(evt => evt.ExpiresAt)
            .IsRequired();

        builder.Property(evt => evt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(evt => evt.Token)
            .IsUnique()
            .HasDatabaseName("IX_EmailVerificationTokens_Token");

        builder.HasIndex(evt => evt.UserId)
            .HasDatabaseName("IX_EmailVerificationTokens_UserId");

        builder.HasIndex(evt => evt.ExpiresAt)
            .HasDatabaseName("IX_EmailVerificationTokens_ExpiresAt");

        // Relationships
        builder.HasOne(evt => evt.User)
            .WithMany(u => u.EmailVerificationTokens)
            .HasForeignKey(evt => evt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
