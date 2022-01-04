using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Utilities;

public static class DataGenerator
{
    public static async Task SeedAsync(Ps7Context context, ILogger logger)
    {
        await context.Database.EnsureCreatedAsync();
    }
}