using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Infrastructure.Identity;

namespace SaaS.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");

        builder.HasKey(session => session.Id);

        builder.Property(session => session.UserId)
            .IsRequired();

        builder.Property(session => session.CreatedAtUtc)
            .IsRequired();

        builder.Property(session => session.ExpiresAtUtc)
            .IsRequired();

        builder.Property(session => session.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(session => session.LastSeenIp)
            .HasMaxLength(64);

        builder.Property(session => session.UserAgent)
            .HasMaxLength(512);

        builder.Property(session => session.DeviceName)
            .HasMaxLength(128);

        builder.HasIndex(session => session.UserId);
        builder.HasIndex(session => session.ExpiresAtUtc);
        builder.HasIndex(session => session.RefreshTokenHash)
            .IsUnique();

        builder.HasOne(session => session.User)
            .WithMany(user => user.Sessions)
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
