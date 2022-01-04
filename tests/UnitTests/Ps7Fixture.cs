using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PS7Api.Models;
using PS7Api.Utilities;
using Xunit.Abstractions;

namespace PS7Api.UnitTests;

public class Ps7Fixture : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped(sp => new DbContextOptionsBuilder<Ps7Context>()
                .UseInMemoryDatabase("TestDb")
                .UseApplicationServiceProvider(sp)
                .Options);
        });
        return base.CreateHost(builder);
    }
}

public static class TestUtilities
{
    private record LoginResult(string Token, DateTime Expiration);
    
    public static void Login(this HttpClient client, string name)
    {
        var response = client.PostAsJsonAsync("/api/Auth/login", new { email = $"{name}@local", password = name }).Result;
        response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<LoginResult>().Result!.Token);
    }
}