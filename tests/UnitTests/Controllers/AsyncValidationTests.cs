using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PS7Api.Models;
using PS7Api.Utilities;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class AsyncValidationTests
{
	
	[Fact]
	public async Task Scan_Documents_CrossingInfo_No_EntryToll()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("customs");
		
		await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo(new TollOffice("fr"))));

		var test = await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo()));
		Assert.Equal(HttpStatusCode.Created, test.StatusCode);

		//add documents
		var contentDoc = new MultipartFormDataContent
			{ { new ByteArrayContent(Array.Empty<byte>()), "file", "image.jpg" } };
		test = await client.PostAsync("/api/CrossingInfo/2/Document", contentDoc);
		Assert.Equal(HttpStatusCode.Created, test.StatusCode);
		var result = await client.GetAsync("/api/CrossingInfo/2");
		var info = result.Content.ReadFromJsonAsync<CrossingInfo>();
		Assert.False(info.Result!.Registered);
		
		var query = new Dictionary<string, string?>
		{
			["id"] = "2",
			["tollId"] = "1"
		};
		result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/2/EntryToll", query),
			JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
		Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
		result = await client.GetAsync("/api/CrossingInfo/2");
		info = result.Content.ReadFromJsonAsync<CrossingInfo>();
		Assert.True(info.Result!.Registered);
	}
	
	[Fact]
	public async Task Add_twice_CrossingInfo_No_EntryToll()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("customs");
		
		await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo(new TollOffice("fr"))));
		
		var test = await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo()));
		Assert.Equal(HttpStatusCode.Created, test.StatusCode);
		
		var query = new Dictionary<string, string?>
		{
			["id"] = "2",
			["tollId"] = "1"
		};
		var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/2/EntryToll", query),
			JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
		Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
		
		query = new Dictionary<string, string?>
		{
			["id"] = "2",
			["tollId"] = "2"
		};
		result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/2/EntryToll", query),
			JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
		Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
	}
	
	[Fact]
	public async Task Add_Second_TollEntry_CrossingInfo()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("customs");
		var test = await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo(new TollOffice("fr"))));
		
		var query = new Dictionary<string, string?>
		{
			["id"] = "1",
			["tollId"] = "2"
		};
		var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/1/EntryToll", query),
			JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));
		Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
	}
	
	[Fact]
	public async Task Cannot_Validate_CrossingInfo_No_EntryToll()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		client.Login("customs");

		var test = await client.PostAsync("/api/CrossingInfo",
			JsonContent.Create(new CrossingInfo()));
		
		var query = new Dictionary<string, string?>
		{
			["id"] = "1",
			["tollId"] = "2"
		};
		var result = await client.PatchAsync(QueryHelpers.AddQueryString("/api/CrossingInfo/", query),
			JsonContent.Create(DateTime.Now.AddDays(1).Iso8601()));

		Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
	}
	
}