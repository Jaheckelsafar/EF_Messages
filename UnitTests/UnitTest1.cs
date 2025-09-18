using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

namespace UnitTests;
    
public partial class Tests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
    }

    [Test]
    public async Task GetThread_ReturnsOk()
    {
        

        var client = _factory.CreateClient();
        //var response = await client.GetAsync("/getthread/1");

        var response = await client.GetAsync("/login?un=john&pw=password123");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.NotFound));


        var contentString = await response.Content.ReadAsStringAsync();

        var blah = JsonDocument.Parse(contentString);
        var token = blah.RootElement.GetProperty("token").GetString();
        var userId = blah.RootElement.GetProperty("userId").GetInt32();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("userId", userId.ToString());


        response = await client.GetAsync("/getthread/1");

        // Assert
         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.NotFound));

        contentString = await response.Content.ReadAsStringAsync();

        Console.WriteLine(contentString);
        // You can add more assertions here based on your expected response
    }



    [TearDown]
    public void Teardown()
    {
        _factory.Dispose();
    }
}
