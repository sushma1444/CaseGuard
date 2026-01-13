using CaseGuard.Backend.Assignment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseGuard.Backend.Assignment.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the LicenseAssignment entity.
/// </summary>
public class LicenseAssignmentConfiguration : IEntityTypeConfiguration<LicenseAssignment>
{
    public void Configure(EntityTypeBuilder<LicenseAssignment> builder)
    {
        builder.ToTable("LicenseAssignments");

        builder.HasKey(la => la.Id);

        builder.Property(la => la.Id)
            .ValueGeneratedOnAdd();

        builder.Property(la => la.AssignedAt)
            .IsRequired();

        builder.Property(la => la.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(la => new { la.LicenseId, la.UserId, la.OrganizationMemberId })
            .IsUnique()
            .HasDatabaseName("IX_LicenseAssignments_LicenseId_UserId_OrganizationMemberId")
            .HasFilter("\"UnassignedAt\" IS NULL");

        builder.HasIndex(la => la.LicenseId)
            .HasDatabaseName("IX_LicenseAssignments_LicenseId");

        builder.HasIndex(la => la.UserId)
            .HasDatabaseName("IX_LicenseAssignments_UserId");

        builder.HasIndex(la => la.OrganizationMemberId)
            .HasDatabaseName("IX_LicenseAssignments_OrganizationMemberId");

        // Relationships
        builder.HasOne(la => la.License)
            .WithMany(l => l.LicenseAssignments)
            .HasForeignKey(la => la.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.User)
            .WithMany(u => u.LicenseAssignments)
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.OrganizationMember)
            .WithMany(om => om.LicenseAssignments)
            .HasForeignKey(la => la.OrganizationMemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
