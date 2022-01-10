using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

    public DbSet<Document> Documents { get; set; } // todo reduntant?
    public DbSet<DocumentAnomaly> DocumentAnomalies { get; set; }
    public DbSet<RequiredDocument> RequiredDocuments { get; set; }
    public DbSet<CrossingInfo> CrossingInfos { get; set; }
}