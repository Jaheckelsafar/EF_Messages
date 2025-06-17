using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using EF_Messages;



namespace UnitTests;

public class Tests
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
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/getthread/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.NotFound));

        var contentString = await response.Content.ReadAsStringAsync();

        Console.WriteLine(contentString);
        // You can add more assertions here based on your expected response
    }

    [TearDown]
    public void Teardown()
    {
        _factory.Dispose();
    }
}
