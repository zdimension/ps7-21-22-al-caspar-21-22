using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PS7Api.Controllers;
using PS7Api.Models;
using PS7Api.Utilities;
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
        var content = new CrossingInfo {EntryToll = new TollOffice("fr")};
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
            //ExitTollTime = new DateTime(2022, 3, 15, 8, 0, 0),
            TypeId = 0,
            EntryTollId = 1
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));
        content = new CrossingInfo
        {
            EntryTollTime = DateTime.Now,
            //ExitTollTime = DateTime.Now.AddDays(1),
            TypeId = 1,
            EntryTollId = 2
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));

        var query = new Dictionary<string, string?>
        {
            ["passengerCountMin"] = "0",
            ["passengerCountMax"] = "4",
            ["startDate"] = DateTime.Now.Iso8601(),
            ["endDate"] = DateTime.Now.AddDays(1).Iso8601(),
            ["passengerType"] = "1",
            ["tollId"] = "2"
        };

        var res = await client.GetAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query));

        var listCrossingInfo = res.Content.ReadFromJsonAsync<List<CrossingInfo>>();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Single(listCrossingInfo.Result!);
    }

    [Fact]
    public async Task Allow_Crossing_Infos_Return_204()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var test = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo{EntryTollId = 1}));
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
 
        //scanning document
        var contentDoc = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        test = await client.PostAsync("/api/CrossingInfo/1/Document", contentDoc);
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
        var result = await client.GetAsync("/api/CrossingInfo/1");
        var info = result.Content.ReadFromJsonAsync<CrossingInfo>();
        Assert.Single(info.Result!.Documents);
        
        //allow crossing
        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
    
    [Fact]
    public async Task Allow_Crossing_Infos_With_Anomalies_Return_403()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var test = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo{EntryTollId = 1}));
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
 
        //scanning document
        var contentDoc = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        test = await client.PostAsync("/api/CrossingInfo/1/Document", contentDoc);
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
        var result = await client.GetAsync("/api/CrossingInfo/1");
        var info = result.Content.ReadFromJsonAsync<CrossingInfo>();
        Assert.Single(info.Result!.Documents);
        
        //generate anomaly
        var anomaliesDesc = new[] { "coin coin", "42", "GRRRR" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //allow crossing
        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task Scan_Invalid_Document_No_Allow()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var test = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo{EntryTollId = 1}));
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
 
        //scanning document
        var contentDoc = new MultipartFormDataContent { { new ByteArrayContent(new byte[42]), "file", "image.jpg" } };
        test = await client.PostAsync("/api/CrossingInfo/1/Document", contentDoc);
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
        var result = await client.GetAsync("/api/CrossingInfo/1");
        var info = result.Content.ReadFromJsonAsync<CrossingInfo>();
        Assert.Single(info.Result!.Documents);
        
        Assert.Equal(2, info.Result.Documents.First().Anomalies.Count);
        
        //allow crossing
        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task Allow_Crossing_No_Documents_Returns_403()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var test = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo{EntryTollId = 1}));
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
        
        //allow crossing
        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task Allow_Crossing_Infos_Twice_Return_209()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var test = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo{EntryTollId = 1}));
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
 
        //scanning document
        var contentDoc = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        test = await client.PostAsync("/api/CrossingInfo/1/Document", contentDoc);
        Assert.Equal(HttpStatusCode.Created, test.StatusCode);
        var result = await client.GetAsync("/api/CrossingInfo/1");
        var info = result.Content.ReadFromJsonAsync<CrossingInfo>();
        Assert.Single(info.Result!.Documents);
        
        //allow crossing
        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        
        query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "3"
        };
        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query), JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }
}