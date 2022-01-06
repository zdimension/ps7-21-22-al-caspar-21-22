using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PS7Api.Models;

public class Ps7Context : IdentityDbContext<User>
{
    public Ps7Context(DbContextOptions<Ps7Context> options) : base(options)
    {
        
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentAnomaly> DocumentAnomalies { get; set; }
    
    //todo trouver pourquoi cette ligne fait tout planter même quand personne s'en sert
    public DbSet<RequiredDocument> RequiredDocuments { get; set; }
    public DbSet<StreamFrontier> StreamsFrontiers { get; set; }
}