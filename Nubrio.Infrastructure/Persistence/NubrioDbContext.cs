using Microsoft.EntityFrameworkCore;
using Nubrio.Domain.Models;
using Nubrio.Infrastructure.Persistence.Configurations;
using Nubrio.Infrastructure.Persistence.Entities;

namespace Nubrio.Infrastructure.Persistence;

public class NubrioDbContext(DbContextOptions<NubrioDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Request> Requests => Set<Request>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
        modelBuilder.ApplyConfiguration(new RequestConfiguration());
    }
}