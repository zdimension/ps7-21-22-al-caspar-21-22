using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

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
    public async Task Posting_Document_Returns_201()
    {
        await using var app = new Ps7Fixture();
    
        var client = app.CreateClient();
        client.Login("customs");
        var content = new MultipartFormDataContent { { new ByteArrayContent(new byte[0]), "file", "document.jpg" } };
        var res = await client.PostAsync("/api/Document", content);
    
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
}