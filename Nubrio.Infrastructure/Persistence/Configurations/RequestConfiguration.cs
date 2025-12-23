using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nubrio.Infrastructure.Persistence.Entities;

namespace Nubrio.Infrastructure.Persistence.Configurations;

public sealed class RequestConfiguration  : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("requests");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Endpoint).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TimestampUtc).IsRequired();
        builder.Property(x => x.CacheHit).IsRequired(false);
        builder.Property(x => x.StatusCode).IsRequired();
        builder.Property(x => x.LatencyMs).IsRequired();
        builder.Property(x => x.Date).IsRequired(false);
        
        builder.HasIndex(x => new  { x.TimestampUtc, x.Id });
        builder.HasIndex(x => new  { x.TimestampUtc, x.City });
    }
}