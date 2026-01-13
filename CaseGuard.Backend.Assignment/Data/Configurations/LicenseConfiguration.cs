using CaseGuard.Backend.Assignment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaseGuard.Backend.Assignment.Data.Configurations;

/// <summary>
/// Entity Framework configuration for the License entity.
/// </summary>
public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.StartDate)
            .IsRequired();

        builder.Property(l => l.ExpirationDate)
            .IsRequired();

        builder.Property(l => l.AutoRenewalEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(l => l.OrganizationId)
            .HasDatabaseName("IX_Licenses_OrganizationId");

        builder.HasIndex(l => new { l.OrganizationId, l.IsActive, l.ExpirationDate })
            .HasDatabaseName("IX_Licenses_OrganizationId_IsActive_ExpirationDate");

        builder.HasIndex(l => l.AutoRenewalEnabled)
            .HasDatabaseName("IX_Licenses_AutoRenewalEnabled")
            .HasFilter("\"AutoRenewalEnabled\" = true AND \"IsActive\" = true");

        // Relationships
        builder.HasOne(l => l.Organization)
            .WithMany(o => o.Licenses)
            .HasForeignKey(l => l.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.LicenseAssignments)
            .WithOne(la => la.License)
            .HasForeignKey(la => la.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
