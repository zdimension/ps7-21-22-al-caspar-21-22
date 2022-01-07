using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PS7Api.Controllers;
using PS7Api.Models;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class CrossingInfoControllerTests
{
    [Fact]
    public async Task Posting_Crossing_Info_Return_201()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        var content = new CrossingInfo();
        var res = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
    
    [Fact]
    public async Task Filtering_Crossing_Infos_Return_200()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        var content = new CrossingInfo
        {
            EntryTollTime = new DateTime(2022, 3, 14, 20, 0, 0),
            ExitTollTime = new DateTime(2022, 3, 15, 8, 0, 0),
            TypeId = 0,
            EntryTollId = 1
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));
        content = new CrossingInfo
        {
            EntryTollTime = DateTime.Now,
            ExitTollTime = DateTime.Now.AddDays(1),
            TypeId = 1,
            EntryTollId = 1
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));
        
        var query = new Dictionary<string, string>
        {
            ["passengerRange"] = new CrossingInfoController.CountRange(0, 4).ToString(),
            ["entryTollTime"] = DateTime.Now.ToString(CultureInfo.CurrentCulture),
            ["exitTollTime"] = DateTime.Now.AddDays(1).ToString(CultureInfo.CurrentCulture),
            ["passengerType"] = "1",
            ["tollId"] = "1"
        };
        
        var passengerCount = new CrossingInfoController.CountRange(0, 4);
        Debug.Write(JsonContent.Create(passengerCount));
        
        var res = await client.GetAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/",  query!));

        var listCrossingInfo = res.Content.ReadFromJsonAsync<List<CrossingInfo>>();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Single(listCrossingInfo.Result!);
    }
}