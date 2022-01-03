using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Utilities;

public static class DataGenerator
{
    public static void Initialize(Ps7Context context)
    {
        context.Database.EnsureCreated();
    }
}