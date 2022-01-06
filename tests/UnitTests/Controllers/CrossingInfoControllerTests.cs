using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
}