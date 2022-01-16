using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PS7Api.Models;

namespace PS7Api.UnitTests;

public class Ps7Fixture : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddDbContext<Ps7Context>(options => options
                .UseInMemoryDatabase(_dbName));
        });
        return base.CreateHost(builder);
    }
}

public static class TestUtilities
{
    public static void Login(this HttpClient client, string name)
    {
        var response = client.PostAsJsonAsync("/api/Auth/login", new { email = $"{name}@local", password = name })
            .Result;
        response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            response.Content.ReadFromJsonAsync<LoginResult>().Result!.Token);
    }

    public static void Logout(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    private record LoginResult(string Token, DateTime Expiration);
}