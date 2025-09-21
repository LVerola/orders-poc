using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Orders.Api.Program>>
{
    private readonly HttpClient _client;

    public HealthCheckTests(WebApplicationFactory<Orders.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_DeveRetornarStatus200()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}