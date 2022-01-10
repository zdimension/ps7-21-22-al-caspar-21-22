using System;
using System.Collections.Generic;
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
    
    const string path = "../../../Image/declaration_douane.png";

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
        
        var res = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = res.Content.ReadFromJsonAsync<CrossingInfo>();

        var imgBytes = await File.ReadAllBytesAsync(path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/Document/"+info.Id, content);

        var result = await client.GetAsync("/api/Document/1");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var doc = await result.Content.ReadFromJsonAsync<Document>();
        Assert.Equal(imgBytes, doc!.Image);
        Assert.NotEqual(DateTime.MinValue, doc.Date);
        Assert.False(doc.Verified);
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
        
        var res = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = res.Content.ReadFromJsonAsync<CrossingInfo>();

        // const string path = "../../../Image/declaration_douane.png";
        var imgBytes = await File.ReadAllBytesAsync(path);
        var content = new MultipartFormDataContent { { new ByteArrayContent(imgBytes), "file", "image.jpg" } };
        await client.PostAsync("/api/Document/"+info.Id, content);

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
        
        var resInfo = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = resInfo.Content.ReadFromJsonAsync<CrossingInfo>();

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document/"+info.Id, content);

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
        
        var resInfo = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = resInfo.Content.ReadFromJsonAsync<CrossingInfo>();

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document/"+info.Id, content);

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
        
        var resInfo = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = resInfo.Content.ReadFromJsonAsync<CrossingInfo>();
        
        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "document.jpg" } };
        var res = await client.PostAsync("/api/Document/"+info.Id, content);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task Deleting_Document_Returns_200()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        
        var resInfo = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(new CrossingInfo()));
        var info = resInfo.Content.ReadFromJsonAsync<CrossingInfo>();
        
        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "document.jpg" } };
        await client.PostAsync("/api/Document/"+info.Id, content);

        var res = await client.DeleteAsync("/api/Document/1");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
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
    public async Task Posting_Document_Without_Authentified()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "document.jpg" } };
        var res = await client.PostAsync("/api/Document", content);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}