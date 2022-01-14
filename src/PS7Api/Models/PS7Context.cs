using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PS7Api.Models;

public class Ps7Context : IdentityDbContext<User>
{
    public Ps7Context(DbContextOptions<Ps7Context> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Human>();
        builder.Entity<Merchendise>();
        
        base.OnModelCreating(builder);
    }

    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentAnomaly> DocumentAnomalies { get; set; } = null!;
    
    public DbSet<RequiredDocument> RequiredDocuments { get; set; } = null!;
    public DbSet<CrossingInfo> CrossingInfos { get; set; } = null!;
    public DbSet<TollOffice> TollOffices { get; set; } = null!;
}