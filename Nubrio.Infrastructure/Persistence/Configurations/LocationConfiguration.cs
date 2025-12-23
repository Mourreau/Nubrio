using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nubrio.Domain.Models;

namespace Nubrio.Infrastructure.Persistence.Configurations;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TimeZoneIana).IsRequired().HasMaxLength(200);
        
        builder.OwnsOne(x => x.Coordinates, coords =>
        {
            coords.Property(x => x.Latitude).HasColumnName("latitude").IsRequired();
            coords.Property(x => x.Longitude).HasColumnName("longitude").IsRequired();
            coords.WithOwner();
        });
        
        builder.OwnsOne(x => x.ExternalLocationId, ext =>
        {
            ext.Property(x => x.ProviderName).HasColumnName("external_provider").IsRequired();
            ext.Property(x => x.Value).HasColumnName("external_id").IsRequired();
            
            ext.HasIndex(
                x => new
                {
                    x.ProviderName, 
                    x.Value
                })
                .IsUnique();
        });

    }
}