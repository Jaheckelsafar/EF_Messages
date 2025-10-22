using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using MessageSystem.Controllers;

namespace UnitTests;
    
public class WebserviceTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
    }

    [Test]
    public async Task UserCreation()
    {
        var client = _factory.CreateClient();

        var userData = new MinimalUserData
        {
            UserName = "",
            Password = "testpass",
            Name = "Test User"
        };
        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/users/register", content);
        Assert.That(response.StatusCode , Is.EqualTo(HttpStatusCode.BadRequest));


        userData = new MinimalUserData
        {
            UserName = "testuser",
            Password = "",
            Name = "Test User"
        };
        json = JsonSerializer.Serialize(userData);
        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        response = await client.PostAsync("/api/users/register", content);
        Assert.That(response.StatusCode , Is.EqualTo(HttpStatusCode.BadRequest));

        userData = new MinimalUserData
        {
            UserName = "testuser",
            Password = "testpass",
            Name = ""
        };
        json = JsonSerializer.Serialize(userData);
        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        response = await client.PostAsync("/api/users/register", content);
        Assert.That(response.StatusCode , Is.EqualTo(HttpStatusCode.BadRequest));

        userData = new MinimalUserData
        {
            UserName = "testuser",
            Password = "testpass",
            Name = "Test User"
        };
        json = JsonSerializer.Serialize(userData);
        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        response = await client.PostAsync("/api/users/register", content);
        Assert.That(response.StatusCode , Is.EqualTo(HttpStatusCode.Created));

        json = JsonSerializer.Serialize(userData);
        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        response = await client.PostAsync("/api/users/register", content);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    // helper to create user for tests
    // returns true if created, false if conflict
    // don't forget to assert that the return value is true in your test -- e.g. Assert.That(createUser(...), Is.True);
    public bool createUser(HttpClient client, string userName, string password, string name)
    {
        var userData = new MinimalUserData
        {
            UserName = userName,
            Password = password,
            Name = name
        };
        var json = JsonSerializer.Serialize(userData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = client.PostAsync("/api/users/register", content).Result;
        return response.StatusCode == HttpStatusCode.Created;
    }

    // helper to login user for tests
    // returns true if login successful, false if not
    // out parameters return the token and userId if successful
    // don't forget to assert that the return value is true in your test -- e.g. Assert.That(loginUser(...), Is.True);
    public bool loginUser(HttpClient client, string userName, string password, out string token, out int userId)
    {
        userId = 0;
        token = "";
        var loginData = new LoginRequest
        {
            UserName = userName,
            Password = password
        };
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = client.PostAsync("/api/users/login", content).Result;
        if (response.StatusCode != HttpStatusCode.OK)
            return false;

        var contentString = response.Content.ReadAsStringAsync().Result;
        var doc = JsonDocument.Parse(contentString);
        token = doc.RootElement.GetProperty("token").GetString() ?? "";
        userId = doc.RootElement.GetProperty("userId").GetInt32();
        return true;
    }
    
    [Test]
    public async Task UserLogin()
    {
        var client = _factory.CreateClient();

        Assert.That(createUser(client, "testuser", "testpass", "Test User"), Is.True);
       
        var loginData = new LoginRequest
        {
            UserName = "testuser",
            Password = "wrongpass"
        };
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/users/login", content);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        loginData = new LoginRequest
        {
            UserName = "testuser",
            Password = "testpass"
        };
        json = JsonSerializer.Serialize(loginData);
        content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        response = await client.PostAsync("/api/users/login", content);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
