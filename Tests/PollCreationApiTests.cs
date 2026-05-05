using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using _1_the_real_time_polling_app_focus_signalr_websockets.Dtos;

namespace RealTimePolling.Tests;

public class PollCreationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PollCreationApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePoll_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "What is your favorite color?",
            Options = new List<string> { "Red", "Blue", "Green" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        Assert.NotNull(result);
        Assert.Equal("What is your favorite color?", result.Question);
        Assert.Equal(3, result.Options.Count);
        Assert.NotEmpty(result.RoomCode);
        Assert.Matches(@"^\d{4}$", result.RoomCode);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreatePoll_WithTwoOptions_Returns201Created()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "Yes or No?",
            Options = new List<string> { "Yes", "No" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Options.Count);
    }

    [Fact]
    public async Task CreatePoll_WithFourOptions_Returns201Created()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "Pick a direction",
            Options = new List<string> { "North", "South", "East", "West" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        Assert.NotNull(result);
        Assert.Equal(4, result.Options.Count);
    }

    [Fact]
    public async Task CreatePoll_WithEmptyQuestion_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "",
            Options = new List<string> { "Option 1", "Option 2" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithWhitespaceQuestion_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "   ",
            Options = new List<string> { "Option 1", "Option 2" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithOneOption_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "What do you think?",
            Options = new List<string> { "Only one option" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithZeroOptions_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "What do you think?",
            Options = new List<string>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithFiveOptions_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "Pick one",
            Options = new List<string> { "A", "B", "C", "D", "E" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithEmptyOptionText_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "What do you prefer?",
            Options = new List<string> { "Valid option", "" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithWhitespaceOptionText_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "What do you prefer?",
            Options = new List<string> { "Valid option", "   " }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_TrimsQuestionAndOptions()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "  Trimmed question  ",
            Options = new List<string> { "  Option A  ", "  Option B  " }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        Assert.NotNull(result);
        Assert.Equal("Trimmed question", result.Question);
        Assert.Equal("Option A", result.Options[0].Text);
        Assert.Equal("Option B", result.Options[1].Text);
    }

    [Fact]
    public async Task CreatePoll_ReturnsLocationHeader()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "Test question",
            Options = new List<string> { "Option 1", "Option 2" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/polls/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreatePoll_OptionsHaveZeroVoteCount()
    {
        // Arrange
        var request = new CreatePollRequest
        {
            Question = "New poll",
            Options = new List<string> { "A", "B" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreatePollResponse>();
        Assert.NotNull(result);
        Assert.All(result.Options, opt => Assert.Equal(0, opt.VoteCount));
    }
}
