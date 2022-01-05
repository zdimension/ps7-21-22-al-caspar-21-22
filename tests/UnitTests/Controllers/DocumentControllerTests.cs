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
    [Fact]
    public async Task Missing_Document_Returns_404()
    {
        await using var app = new Ps7Fixture();
        
        var client = app.CreateClient();
        var doc = await client.GetAsync("/api/Document/0");
        
        Assert.Equal(HttpStatusCode.NotFound, doc.StatusCode);
    }

    [Fact]
    public async Task Anomalies_Added()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        
        var content = new MultipartFormDataContent { { new ByteArrayContent(new byte[0]), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);

        var anomaliesDesc = new[] {"coin coin", "42", "GRRRR"};
        var anomalies = new DocumentController.Anomalies(anomaliesDesc);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var response = await client.GetAsync("/api/Document/1");
        var doc = await response.Content.ReadFromJsonAsync<Document>();
        
        Assert.Equal(3, doc.Anomalies.Count);
        Assert.Equal(anomaliesDesc, doc.Anomalies.Select(anomaly => anomaly.Anomaly));
    }
    
    [Fact]
    public async Task Anomalies_Empty()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");
        
        var content = new MultipartFormDataContent { { new ByteArrayContent(new byte[0]), "file", "image.jpg" } };
        await client.PostAsync("/api/Document", content);
        
        var anomalies = new DocumentController.Anomalies(new string[0]);
        var res = await client.PostAsync("/api/Document/1/Non-compliant", JsonContent.Create(anomalies));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, res.StatusCode);

        var response = await client.GetAsync("/api/Document/1");
        var doc = await response.Content.ReadFromJsonAsync<Document>();
        
        Assert.Equal(0, doc.Anomalies.Count);
    }
    
    [Fact]
    public async Task Posting_Document_Returns_201()
    {
        await using var app = new Ps7Fixture();
    
        var client = app.CreateClient();
        client.Login("customs");
        var content = new MultipartFormDataContent { { new ByteArrayContent(new byte[0]), "file", "document.jpg" } };
        var res = await client.PostAsync("/api/Document", content);
    
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
    
    [Fact]
    public async Task Deleting_Document_Returns_200()
    {
        await using var app = new Ps7Fixture();
    
        var client = app.CreateClient();
        client.Login("customs");
        var content = new MultipartFormDataContent { { new ByteArrayContent(new byte[0]), "file", "document.jpg" } };
        await client.PostAsync("/api/Document", content);

        var res = await client.DeleteAsync(("/api/Document/1"));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
    
    [Fact]
    public async Task Deleting_Document_Returns_404()
    {
        await using var app = new Ps7Fixture();
    
        var client = app.CreateClient();
        client.Login("customs");

        var res = await client.DeleteAsync(("/api/Document/1"));
        
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}