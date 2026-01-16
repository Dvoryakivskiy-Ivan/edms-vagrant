using System.Net;
using EDMS.MvcClient.Models;
using Xunit;

namespace EDMS.MvcClientTests;

public class MvcClientSmokeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MvcClientSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    // Test 1: Departments directory list opens
    [Fact]
    public async Task Departments_Index_Returns200()
    {
        using var client = CreateClient();
        var resp = await client.GetAsync("/Departments");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var html = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Departments", html, StringComparison.OrdinalIgnoreCase);
    }

    // Test 2: Documents main list opens (central table list)
    [Fact]
    public async Task Documents_Browse_Returns200()
    {
        using var client = CreateClient();
        var resp = await client.GetAsync("/Documents/Browse");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var html = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Documents", html, StringComparison.OrdinalIgnoreCase);
    }

    // Test 3: Create page opens (authorized, admin)
    [Fact]
    public async Task Documents_Create_Get_Returns200()
    {
        using var client = CreateClient();
        var resp = await client.GetAsync("/Documents/Create");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var html = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Create", html, StringComparison.OrdinalIgnoreCase);
    }

   
}
