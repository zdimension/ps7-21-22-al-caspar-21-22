using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PS7Api.Models;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class PassControllerTest
{
    [Fact]
    public async Task Fr_To_Gb()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        var now = DateTime.Now;

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = now,
            ExitTollTime = now,
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        var cross2 = new CrossingInfo(new TollOffice("gb"))
        {
            ExitToll = new TollOffice("fr"),
            EntryTollTime = now,
            ExitTollTime = now,
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));

        var res = await client.GetAsync("/api/Pass?from=fr&to=gb");
        var passResult = await res.Content.ReadFromJsonAsync<CrossingInfo[]>();
        Assert.Single(passResult!);
        var resCross = passResult![0];
        Assert.Equal("fr", resCross.EntryToll.Country);
        Assert.Equal("gb", resCross.ExitToll!.Country);
    }

    [Fact]
    public async Task Gb_To_Fr()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        var now = DateTime.Now;

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = now,
            ExitTollTime = now,
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        var cross2 = new CrossingInfo(new TollOffice("gb"))
        {
            ExitToll = new TollOffice("fr"),
            EntryTollTime = now,
            ExitTollTime = now,
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));

        var res = await client.GetAsync("/api/Pass?from=gb&to=fr");
        var passResult = await res.Content.ReadFromJsonAsync<CrossingInfo[]>();
        Assert.Single(passResult!);
        var resCross = passResult![0];
        Assert.Equal("gb", resCross.EntryToll.Country);
        Assert.Equal("fr", resCross.ExitToll!.Country);
    }

    [Fact]
    public async Task No_argument()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        var now = DateTime.Now;

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = now,
            ExitTollTime = now,
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        var cross2 = new CrossingInfo(new TollOffice("gb"))
        {
            ExitToll = new TollOffice("fr"),
            EntryTollTime = now,
            ExitTollTime = now,
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));

        var res = await client.GetAsync("/api/Pass");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Missing_to()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        var now = DateTime.Now;

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = now,
            ExitTollTime = now,
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        var cross2 = new CrossingInfo(new TollOffice("gb"))
        {
            ExitToll = new TollOffice("fr"),
            EntryTollTime = now,
            ExitTollTime = now,
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));

        var res = await client.GetAsync("/api/Pass?from=fr");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }


    [Fact]
    public async Task Missing_from()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        var now = DateTime.Now;

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = now,
            ExitTollTime = now,
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        var cross2 = new CrossingInfo(new TollOffice("gb"))
        {
            ExitToll = new TollOffice("fr"),
            EntryTollTime = now,
            ExitTollTime = now,
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));

        var res = await client.GetAsync("/api/Pass?to=fr");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task With_start()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 10, 12, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 12, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross2 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 10, 12, 2, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 12, 37, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross3 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 10, 11, 23, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 11, 58, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross3));

        var res = await client.GetAsync("/api/Pass?from=fr&to=gb&start=2022-1-10T12:00:00");
        var passResult = await res.Content.ReadFromJsonAsync<CrossingInfo[]>();
        Assert.Equal(2, passResult!.Length);
        var entryTollTimes = passResult.Select(info => info.EntryTollTime).ToList();
        Assert.Contains(cross1.EntryTollTime, entryTollTimes);
        Assert.Contains(cross2.EntryTollTime, entryTollTimes);
    }

    [Fact]
    public async Task With_datetime_interval()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 12, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 12, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross2 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross3 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 11, 58, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 8, 11, 23, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross4 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 5, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 40, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross3));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross4));

        var res = await client.GetAsync("/api/Pass?from=fr&to=gb&start=2022-01-08T12:00:00&end=2022-01-09T11:00:00");
        var passResult = await res.Content.ReadFromJsonAsync<CrossingInfo[]>();
        Assert.Equal(2, passResult!.Length);
        var entryTollTimes = passResult.Select(info => info.EntryTollTime).ToList();
        Assert.Contains(cross1.EntryTollTime, entryTollTimes);
        Assert.Contains(cross2.EntryTollTime, entryTollTimes);
        // Assert.Contains(cross3.EntryTollTime, entryTollTimes);
    }

    [Fact]
    public async Task End_without_start()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 12, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 12, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross2 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross3 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 11, 58, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 8, 11, 23, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross4 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 5, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 40, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross3));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross4));

        var res = await client.GetAsync("/api/Pass?from=fr&to=gb&end=2022-01-09T11:00:00");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
    }

    [Fact]
    public async Task End_before_start()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var cross1 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 12, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 10, 12, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross2 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 0, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 35, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross3 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 8, 11, 58, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 8, 11, 23, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };
        var cross4 = new CrossingInfo(new TollOffice("fr"))
        {
            EntryTollTime = new DateTime(2022, 1, 9, 11, 5, 0, 0),
            ExitTollTime = new DateTime(2022, 1, 9, 11, 40, 0, 0),
            ExitToll = new TollOffice("gb"),
            Transport = Transport.Car
        };

        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross1));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross2));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross3));
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(cross4));

        var res = await client.GetAsync("/api/Pass?from=fr&to=gb&start=2022-01-09T12:00:00&end=2022-01-08T11:00:00");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);
    }
}