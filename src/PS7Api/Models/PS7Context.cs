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
        builder.Entity<StreamFrontier>().Property(p => p.CrossingPoints)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions())!)
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1 == c2 || c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Entity<Human>();
        builder.Entity<Merchendise>();
        
        base.OnModelCreating(builder);
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentAnomaly> DocumentAnomalies { get; set; }
    
    //todo trouver pourquoi cette ligne fait tout planter même quand personne s'en sert
    public DbSet<RequiredDocument> RequiredDocuments { get; set; }
    public DbSet<StreamFrontier> StreamsFrontiers { get; set; }
}