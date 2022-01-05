using System.Net;
using System.Threading.Tasks;
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
		
		Assert.Equal(HttpStatusCode.OK, allDocAno.StatusCode);
	}
	
	[Fact]
	public async Task Missing_DocumentAnomaly_GET_Returns_404()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("admin");
		var docAno = await client.GetAsync("/api/DocumentAnomaly/0");
		
		Assert.Equal(HttpStatusCode.NotFound, docAno.StatusCode);
	}
	
	[Fact]
	public async Task Missing_DocumentAnomaly_DELETE_Returns_404()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("admin");
		var docAno = await client.DeleteAsync("/api/DocumentAnomaly/0");
		
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
		var docAno = await client.GetAsync("/api/DocumentAnomaly/0");
		
		Assert.Equal(HttpStatusCode.Unauthorized, docAno.StatusCode);
	}
	
	[Fact]
	public async Task Missing_DocumentAnomaly_DELETE_Returns_401()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		var docAno = await client.DeleteAsync("/api/DocumentAnomaly/0");
		
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
	
	
}