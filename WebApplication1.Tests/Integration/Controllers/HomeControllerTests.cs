using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

public class HomeControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Privacy")]
    public async Task Get_Endpoints_ReturnSuccessAndView(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }
}
