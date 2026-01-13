using CaseGuard.Backend.Assignment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseGuard.Backend.Assignment.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the Invitation entity.
/// </summary>
public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(i => new { i.OrganizationId, i.Email, i.Status })
            .HasDatabaseName("IX_Invitations_OrganizationId_Email_Status");

        builder.HasIndex(i => i.Email)
            .HasDatabaseName("IX_Invitations_Email");

        builder.HasIndex(i => i.OrganizationId)
            .HasDatabaseName("IX_Invitations_OrganizationId");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_Invitations_Status")
            .HasFilter("\"Status\" = 0"); // Filter for pending invitations

        // Relationships
        builder.HasOne(i => i.Organization)
            .WithMany(o => o.Invitations)
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.User)
            .WithMany(u => u.Invitations)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
