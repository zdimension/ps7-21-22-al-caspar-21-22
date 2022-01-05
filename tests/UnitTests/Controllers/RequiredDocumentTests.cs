using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class RequiredDocumentTests
{
	[Fact]
	public async Task Get_Docs_FR_FromFR_ToEN_Ok()
	{
		await using var app = new Ps7Fixture();

		var client = app.CreateClient();
		var query = new Dictionary<string, string>
		{
			["nationality"] = "fr-FR",
			["origin"] = "fr-FR",
			["destination"] = "en-GB",
		};
		var response = await client.GetAsync(QueryHelpers.AddQueryString("/api/RequiredDocument/", query));
		var resData = await response.Content.ReadFromJsonAsync<List<string>>();

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(4, resData.Count);
	}
}