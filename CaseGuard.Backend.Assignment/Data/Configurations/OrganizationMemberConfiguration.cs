using CaseGuard.Backend.Assignment.Entities;
using CaseGuard.Backend.Assignment.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseGuard.Backend.Assignment.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the OrganizationMember entity.
/// </summary>
public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("OrganizationMembers");

        builder.HasKey(om => om.Id);

        builder.Property(om => om.Id)
            .ValueGeneratedOnAdd();

        builder.Property(om => om.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(om => om.JoinedAt)
            .IsRequired();

        builder.Property(om => om.CreatedAt)
            .IsRequired();

        builder.Property(om => om.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(om => new { om.UserId, om.OrganizationId })
            .IsUnique()
            .HasDatabaseName("IX_OrganizationMembers_UserId_OrganizationId");

        builder.HasIndex(om => om.OrganizationId)
            .HasDatabaseName("IX_OrganizationMembers_OrganizationId");

        builder.HasIndex(om => om.UserId)
            .HasDatabaseName("IX_OrganizationMembers_UserId");

        // Relationships
        builder.HasOne(om => om.User)
            .WithMany(u => u.OrganizationMembers)
            .HasForeignKey(om => om.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(om => om.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(om => om.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(om => om.LicenseAssignments)
            .WithOne(la => la.OrganizationMember)
            .HasForeignKey(la => la.OrganizationMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
