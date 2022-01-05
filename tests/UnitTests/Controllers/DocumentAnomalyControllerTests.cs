using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PS7Api.Controllers;
using PS7Api.Models;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class DocumentAnomalyControllerTests
{
    [Fact]
    public async Task Empty_DocumentAnomaly_GET_Returns_200()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("admin");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly");
        var res = await allDocAno.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();

        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
        Assert.Empty(res!);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_GET_Returns_404()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("admin");
        var docAno = await client.GetAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.NotFound, docAno.StatusCode);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_DELETE_Returns_404()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("admin");
        var docAno = await client.DeleteAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.NotFound, docAno.StatusCode);
    }

    [Fact]
    public async Task Empty_DocumentAnomaly_GET_Returns_401()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly");

        Assert.Equal(HttpStatusCode.Unauthorized, allDocAno.StatusCode);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_GET_Returns_401()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var docAno = await client.GetAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.Unauthorized, docAno.StatusCode);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_DELETE_Returns_401()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        var docAno = await client.DeleteAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.Unauthorized, docAno.StatusCode);
    }

    [Fact]
    public async Task Empty_DocumentAnomaly_GET_Returns_403()
    {
        await using var app = new Ps7Fixture();

        var client = app.CreateClient();
        client.Login("customs");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly");

        Assert.Equal(HttpStatusCode.Forbidden, allDocAno.StatusCode);
    }

    [Fact]
    public async Task DocumentAnomaly_GET_Returns_200()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly");
        var resAno = await allDocAno.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();

        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
        Assert.Single(resAno!);
    }

    [Fact]
    public async Task DocumentAnomaly_GET_One_Returns_200()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_GET_One_Returns_404()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.GetAsync("/api/DocumentAnomaly/2");

        Assert.Equal(HttpStatusCode.NotFound, allDocAno.StatusCode);
    }

    [Fact]
    public async Task DocumentAnomaly_DELETE_One_Returns_200()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.DeleteAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);

        var docs = await client.GetAsync("/api/DocumentAnomaly");
        var resAno = await docs.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();

        Assert.Empty(resAno!);
    }

    [Fact]
    public async Task Missing_DocumentAnomaly_DELETE_One_Returns_404()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.DeleteAsync("/api/DocumentAnomaly/2");

        Assert.Equal(HttpStatusCode.NotFound, allDocAno.StatusCode);

        var docs = await client.GetAsync("/api/DocumentAnomaly");
        var resAno = await docs.Content.ReadFromJsonAsync<List<DocumentAnomaly>>();

        Assert.Single(resAno!);
    }

    [Fact]
    public async Task DocumentAnomaly_DELETE_Twice_Returns_404()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var content = new MultipartFormDataContent { { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] { "foo" };
        var anomalies = new DocumentController.AnomaliesBody(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        res.EnsureSuccessStatusCode();

        client.Login("admin");
        var allDocAno = await client.DeleteAsync("/api/DocumentAnomaly/1");
        var allDocAno2 = await client.DeleteAsync("/api/DocumentAnomaly/1");

        Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, allDocAno2.StatusCode);
    }
}