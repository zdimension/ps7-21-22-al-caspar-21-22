using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PS7Api.Controllers;
using PS7Api.Models;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class DocumentControllerTests
{
    private const string Path = "../../../Image/declaration_douane.png";

    [Fact]
    public async Task Missing_Document_Returns_404()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var doc = await client.GetAsync("/api/Document/0");

        Assert.Equal(HttpStatusCode.NotFound, doc.StatusCode);
    }

    [Fact]
    public async Task Get_Document()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        var result = await client.GetAsync("/api/Document/" + crossInfoId);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var doc = await result.Content.ReadFromJsonAsync<Document>();
        Assert.Equal(imgBytes, doc!.Image);
        Assert.NotEqual(DateTime.MinValue, doc.Date);
        Assert.True(doc.Verified);
        Assert.Empty(doc.Anomalies);
    }

    [Fact]
    public async Task Missing_Image_Returns_404()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var doc = await client.GetAsync("/api/Document/0/Image");

        Assert.Equal(HttpStatusCode.NotFound, doc.StatusCode);
    }

    [Fact]
    public async Task Get_Image()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        // const string path = "../../../Image/declaration_douane.png";
        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        var result = await client.GetAsync("/api/Document/1/Image");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var contentImg = await result.Content.ReadAsByteArrayAsync();
        Assert.Equal(imgBytes, contentImg);
    }

    [Fact]
    public async Task Anomalies_Added()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        var anomaliesDesc = new[] { "coin coin", "42", "GRRRR" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var response = await client.GetAsync("/api/Document/1");
        var doc = await response.Content.ReadFromJsonAsync<Document>();

        Assert.Equal(3, doc!.Anomalies.Count);
        Assert.Equal(anomaliesDesc, doc.Anomalies.Select(anomaly => anomaly.Anomaly));
    }

    [Fact]
    public async Task Anomalies_Empty()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        var anomalies = new DocumentController.AnomaliesBody(Array.Empty<string>());
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);

        var response = await client.GetAsync("/api/Document/1");
        var doc = await response.Content.ReadFromJsonAsync<Document>();

        Assert.Equal(0, doc!.Anomalies.Count);
    }

    [Fact]
    public async Task Posting_Document_Returns_201()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        var res = await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task Deleting_Document_Returns_200()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        var res = await client.DeleteAsync("/api/Document/1");

        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
    }

    [Fact]
    public async Task Deleting_Document_Returns_404()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");

        var res = await client.DeleteAsync("/api/Document/1");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Posting_Document_Without_Authenticated()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var crossInfo = new CrossingInfo(new TollOffice("fr"));
        var resp = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(crossInfo));
        var crossInfoResp = await resp.Content.ReadFromJsonAsync<CrossingInfo>();
        var crossInfoId = crossInfoResp?.Id;

        client.Logout();
        var imgBytes = await File.ReadAllBytesAsync(Path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        var res = await client.PostAsync("/api/CrossingInfo/" + crossInfoId + "/Document", content);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}