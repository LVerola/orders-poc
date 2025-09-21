using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Orders.Api;

public class AnalyticsControllerTests : IClassFixture<WebApplicationFactory<Orders.Api.Program>>
{
    private readonly HttpClient _client;

    public AnalyticsControllerTests(WebApplicationFactory<Orders.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AskEndpoint_DeveRetornarCampoAnswer()
    {
        var json = "{\"Question\":\"Qual o status dos pedidos?\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/analytics/ask", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("answer", body);
    }
}