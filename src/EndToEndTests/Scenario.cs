using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PS7Api.Controllers;
using PS7Api.Models;
using PS7Api.UnitTests;
using PS7Api.Utilities;
using Xunit;

namespace EndToEndTests;

public class Scenario
{
    [Fact]
    public async Task S1()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        //juliette récupère la liste des documents

        var queryJuliette = new Dictionary<string, string>
        {
            ["nationality"] = "FR",
            ["origin"] = "FR",
            ["destination"] = "GB"
        };
        var responseJuliette =
            await client.GetAsync(QueryHelpers.AddQueryString("/api/RequiredDocument/", queryJuliette));
        var resJuliette = await responseJuliette.Content.ReadFromJsonAsync<List<string>>();

        Assert.Equal(HttpStatusCode.OK, responseJuliette.StatusCode);
        Assert.Equal(4, resJuliette.Count);

        //Leslie scan le document de juliette puis elle passe

        client.Login("customs");
        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync("../../../Image/declaration_douane.png");
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        var res = await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        //Lucien consulte les flux
        res = await client.GetAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query));
        var listCrossingInfo = res.Content.ReadFromJsonAsync<List<CrossingInfo>>();
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Single(listCrossingInfo.Result!);
    }

    [Fact]
    public async Task S2()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        //Leslie scan un document qui est invalide

        client.Login("customs");
        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync("../../../Image/declaration_douane.png");
        var contentAno = new MultipartFormDataContent { { new ByteArrayContent(new byte[42]), "file", "image.jpg" } };
        var res = await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", contentAno);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        //Leslie scan un document qui est valide
        crossInfo = new CrossingInfo(new TollOffice("fr"));
        resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        crossInfoId = crossInfoResp?.Id;

        imgBytes = await File.ReadAllBytesAsync("../../../Image/declaration_douane.png");
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        res = await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        query = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "2"
        };
        //Leslie signale une anomalie
        var anomaliesDesc = new[]
            { "Nature invalide : Produits chimiques", "Masse invalide : 5465 Kg", "Peu collaboratif" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));

        result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);


        //Sebastian gère les anomalies
        client.Login("admin");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly");
        var resAnomaly = await allDocAno.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();
        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
        Assert.Equal(4, resAnomaly!.Count);
        var deleteAno = await client.DeleteAsync("/api/DocumentAnomaly/3");
        Assert.Equal(HttpStatusCode.NoContent, deleteAno.StatusCode);
        allDocAno = await client.GetAsync("/api/DocumentAnomaly");
        resAnomaly = await allDocAno.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();
        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
        Assert.Equal(3, resAnomaly!.Count);
    }

    [Fact]
    public async Task S3()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        // ----------------------------------------
        var content = new CrossingInfo(new TollOffice("fr"))
        {
            NbPassengers = 6,
            TypeId = 0,
            EntryTollTime = new DateTime(2022, 3, 14, 20, 0, 0),
            ExitTollTime = new DateTime(2022, 3, 15, 8, 0, 0),
            EntryTollId = 3,
            ExitTollId = 4,
            Transport = Transport.Car
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));
        var contentDoc = new MultipartFormDataContent
            { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/1/Document", contentDoc);
        var validate = new Dictionary<string, string?>
        {
            ["id"] = "1",
            ["tollId"] = "4"
        };
        await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", validate),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));


        // ----------------------------------------
        content = new CrossingInfo(new TollOffice("fr"))
        {
            NbPassengers = 4,
            TypeId = 0,
            EntryTollTime = DateTime.Now,
            ExitTollTime = DateTime.Now.AddDays(1),
            EntryTollId = 2,
            ExitTollId = 1,
            Transport = Transport.Car
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));

        contentDoc = new MultipartFormDataContent
            { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/2/Document", contentDoc);
        validate = new Dictionary<string, string?>
        {
            ["id"] = "2",
            ["tollId"] = "1"
        };
        await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", validate),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        

        // ----------------------------------------
        content = new CrossingInfo(new TollOffice("fr"))
        {
            NbPassengers = 2,
            TypeId = 0,
            EntryTollTime = DateTime.Now,
            ExitTollTime = DateTime.Now.AddDays(1),
            EntryTollId = 2,
            ExitTollId = 1,
            Transport = Transport.Car
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));

        contentDoc = new MultipartFormDataContent
            { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/3/Document", contentDoc);
        validate = new Dictionary<string, string?>
        {
            ["id"] = "2",
            ["tollId"] = "1"
        };
        await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", validate),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        
        // ----------------------------------------
        content = new CrossingInfo(new TollOffice("fr"))
        {
            NbPassengers = 1,
            TypeId = 1,
            EntryTollTime = DateTime.Now,
            ExitTollTime = DateTime.Now.AddDays(1),
            EntryTollId = 2,
            ExitTollId = 1,
            Transport = Transport.Truck
        };
        await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));

        contentDoc = new MultipartFormDataContent
            { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/3/Document", contentDoc);
        validate = new Dictionary<string, string?>
        {
            ["id"] = "2",
            ["tollId"] = "1"
        };
        await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", validate),
            JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
        var query = new Dictionary<string, string?>
        {
            ["validatedCrossing"] = "false",
            ["passengerCountMin"] = "0",
            ["passengerCountMax"] = "4",
            ["startDate"] = DateTime.Now.Iso8601(),
            ["endDate"] = DateTime.Now.AddDays(1).Iso8601(),
            ["passengerType"] = "1",
            ["tollId"] = "1"
        };

        var res = await client.GetAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query));

        var listCrossingInfo = res.Content.ReadFromJsonAsync<List<CrossingInfo>>();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Single(listCrossingInfo.Result!);
    }
}