using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using PS7Api.Models;
using Xunit;

namespace PS7Api.UnitTests.Controllers;

public class PersonControllerTest
{
    
    private const string PersonOneImagePath = "../../../Image/person_1.jpg";
    private const string PersonTwoImagePath = "../../../Image/person_2.jpg";
    private const string PersonThreeImagePath = "../../../Image/person_3.jpg";

    [Fact]
    public async void Create_Person()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        var imgBytes = await File.ReadAllBytesAsync(PersonOneImagePath);
        var data = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes), "photoFile", "person.jpg" } };
        var rep = await client.PostAsync("/api/Person", data);
        Assert.Equal(HttpStatusCode.Created, rep.StatusCode);
    }
    
    [Fact]
    public async void Get_Person_Exist()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        var imgBytes = await File.ReadAllBytesAsync(PersonOneImagePath);
        var data = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes), "photoFile", "person.jpg" } };
        var repPerson = await client.PostAsync("/api/Person", data);
        var person = await repPerson.Content.ReadFromJsonAsync<Person>();
        
        var rep = await client.PostAsync("/api/Person/GetPhoto", data);
        var repId = await rep.Content.ReadAsStringAsync();
        
        Assert.Equal(HttpStatusCode.OK, rep.StatusCode);
        Assert.Equal(person?.Id.ToString(), repId);
    }
    
    [Fact]
    public async void Get_Person_Exist_With_Multiple_Other_Persons()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        var imgBytes1 = await File.ReadAllBytesAsync(PersonOneImagePath);
        var imgBytes2 = await File.ReadAllBytesAsync(PersonTwoImagePath);
        var imgBytes3 = await File.ReadAllBytesAsync(PersonThreeImagePath);
        var data1 = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes1), "photoFile", "person.jpg" } };
        var data2 = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes2), "photoFile", "person.jpg" } };
        var data3 = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes3), "photoFile", "person.jpg" } };
        var repPerson = await client.PostAsync("/api/Person", data1);
        await client.PostAsync("/api/Person", data2);
        await client.PostAsync("/api/Person", data3);
        var person = await repPerson.Content.ReadFromJsonAsync<Person>();
        
        var rep = await client.PostAsync("/api/Person/GetPhoto", data1);
        var repId = await rep.Content.ReadAsStringAsync();
        
        Assert.Equal(HttpStatusCode.OK, rep.StatusCode);
        Assert.Equal(person?.Id.ToString(), repId);
    }
    
    [Fact]
    public async void Person_Doesnt_Exist()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        var imgBytes1 = await File.ReadAllBytesAsync(PersonOneImagePath);
        var imgBytes2 = await File.ReadAllBytesAsync(PersonTwoImagePath);
        var data1 = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes1), "photoFile", "person.jpg" } };
        var data2 = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes2), "photoFile", "person.jpg" } };
        await client.PostAsync("/api/Person", data1);
        
        var rep = await client.PostAsync("/api/Person/GetPhoto", data2);
        
        Assert.Equal(HttpStatusCode.NotFound, rep.StatusCode);
    }

    [Fact]
    public async void Get_Person_Crossing_Info()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        client.Login("customs");

        var imgBytes = await File.ReadAllBytesAsync(PersonOneImagePath);
        var data = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes), "photoFile", "person.jpg" } };
        var repPerson = await client.PostAsync("/api/Person", data);
        var person = await repPerson.Content.ReadFromJsonAsync<Person>();
        
        var content = new CrossingInfo(new TollOffice("fr"), person!.Id);
        var res = await client.PostAsync("/api/CrossingInfo", JsonContent.Create(content));
        var crossInfo = await res.Content.ReadFromJsonAsync<CrossingInfo>();
        
        var repCi = await client.GetAsync("/api/Person/" + person.Id);
        Assert.Equal(HttpStatusCode.OK, repCi.StatusCode);
        
        var crossInfos = await repCi.Content.ReadFromJsonAsync<List<CrossingInfo>>();
        Assert.Single(crossInfos!);
        Assert.Equal(crossInfo!.Id, crossInfos![0].Id);
    }
    
    [Fact]
    public async void Get_Person_Not_Found()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();

        var repCi = await client.GetAsync("/api/Person/0");
        Assert.Equal(HttpStatusCode.NotFound, repCi.StatusCode);
    }

    //Ne fonctionne pas...
    /*[Fact]
    public async void Patch_Not_Found()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        
        var imgBytes = await File.ReadAllBytesAsync(PersonOneImagePath);
        var imgBytes2 = await File.ReadAllBytesAsync(PersonTwoImagePath);
        var data = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes), "photoFile", "person.jpg" } };
        await client.PostAsync("/api/Person", data);
        // var personId = (await rep.Content.ReadFromJsonAsync<Person>())?.Id;

        var patch = new JsonPatchDocument<Person>();
        patch.Replace(person => person.Image, imgBytes2);
        var content = new StringContent(JsonConvert.SerializeObject(patch), Encoding.UTF8, MediaTypeNames.Application.Json);
        var repPatch = await client.PatchAsync("/api/Person/5", content);
        
        // var test2 = await test.ReadAsStringAsync();
        // var test3 = JsonContent.Create(patch);
        // var test4 = await test3.ReadAsStringAsync();
        var test5 = await repPatch.Content.ReadAsStringAsync();

        // var test6 = JsonConvert.DeserializeObject<JsonPatchDocument<Person>>(test2);

        Assert.Equal(HttpStatusCode.NotFound, repPatch.StatusCode);
    }
    
    [Fact]
    public async void Patch_Success()
    {
        await using var app = new Ps7Fixture();
        var client = app.CreateClient();
        
        var imgBytes = await File.ReadAllBytesAsync(PersonOneImagePath);
        var imgBytes2 = await File.ReadAllBytesAsync(PersonTwoImagePath);
        var data = new MultipartFormDataContent{ { new ByteArrayContent(imgBytes), "photoFile", "person.jpg" } };
        var rep = await client.PostAsync("/api/Person", data);
        var personId = (await rep.Content.ReadFromJsonAsync<Person>())?.Id;

        var repPatch = await client.PatchAsync("/api/Person/" + personId, JsonContent.Create(new Person() {Image = imgBytes2}));
        Assert.Equal(HttpStatusCode.NoContent, repPatch.StatusCode);
    }*/
    
}