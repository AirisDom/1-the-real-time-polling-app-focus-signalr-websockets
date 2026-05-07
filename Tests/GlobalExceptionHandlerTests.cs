using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using _1_the_real_time_polling_app_focus_signalr_websockets.Middleware;

namespace RealTimePolling.Tests;

public class GlobalExceptionHandlerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GlobalExceptionHandlerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPoll_NotFound_ReturnsConsistentErrorJson()
    {
        var response = await _client.GetAsync("/api/polls/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content.ToLower());
    }

    [Fact]
    public async Task Vote_InvalidOption_ReturnsConsistentErrorJson()
    {
        var createRequest = new { question = "Test?", options = new[] { "A", "B" } };
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var poll = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        string roomCode = poll.GetProperty("roomCode").GetString()!;

        var voteRequest = new { optionId = 99999, voterId = "test" };
        var voteResponse = await _client.PostAsJsonAsync($"/api/polls/{roomCode}/vote", voteRequest);

        Assert.Equal(HttpStatusCode.BadRequest, voteResponse.StatusCode);

        var content = await voteResponse.Content.ReadAsStringAsync();
        Assert.Contains("error", content.ToLower());
    }

    [Fact]
    public async Task CreatePoll_ValidationError_ReturnsErrorsArray()
    {
        var request = new { question = "", options = new[] { "A" } };
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", content.ToLower());
    }
}
