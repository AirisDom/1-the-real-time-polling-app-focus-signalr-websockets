using _1_the_real_time_polling_app_focus_signalr_websockets.Dtos;
using _1_the_real_time_polling_app_focus_signalr_websockets.Hubs;
using _1_the_real_time_polling_app_focus_signalr_websockets.Middleware;
using _1_the_real_time_polling_app_focus_signalr_websockets.Models;
using _1_the_real_time_polling_app_focus_signalr_websockets.Repositories;
using _1_the_real_time_polling_app_focus_signalr_websockets.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IPollRepository, InMemoryPollRepository>();
builder.Services.AddSingleton<IRoomCodeGenerator, RoomCodeGenerator>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCors", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("SignalRCors");

app.MapHub<PollHub>("/hubs/poll");

app.MapPost("/api/polls", (CreatePollRequest request, IPollRepository repository, IRoomCodeGenerator codeGenerator) =>
{
    var validationErrors = new List<string>();

    if (string.IsNullOrWhiteSpace(request.Question))
    {
        validationErrors.Add("Question is required");
    }

    if (request.Options == null || request.Options.Count < 2)
    {
        validationErrors.Add("At least 2 options are required");
    }
    else if (request.Options.Count > 4)
    {
        validationErrors.Add("Maximum 4 options are allowed");
    }
    else
    {
        for (int i = 0; i < request.Options.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(request.Options[i]))
            {
                validationErrors.Add($"Option {i + 1} text cannot be empty");
            }
        }
    }

    if (validationErrors.Any())
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var roomCode = codeGenerator.GenerateUniqueCode();
    var poll = new Poll
    {
        Question = request.Question.Trim(),
        RoomCode = roomCode,
        Options = request.Options!.Select(optionText => new PollOption
        {
            Text = optionText.Trim()
        }).ToList()
    };

    var createdPoll = repository.CreatePoll(poll);
    var response = new CreatePollResponse
    {
        Id = createdPoll.Id,
        RoomCode = createdPoll.RoomCode,
        Question = createdPoll.Question,
        CreatedAt = createdPoll.CreatedAt,
        Options = createdPoll.Options.Select(o => new PollOptionResponse
        {
            Id = o.Id,
            Text = o.Text,
            VoteCount = o.VoteCount
        }).ToList()
    };

    return Results.Created($"/api/polls/{createdPoll.RoomCode}", response);
})
.WithName("CreatePoll")
.WithDescription("Create a new poll with a question and 2-4 options");

app.MapGet("/api/polls/{roomCode}", (string roomCode, IPollRepository repository) =>
{
    var poll = repository.GetByRoomCode(roomCode);
    if (poll == null)
    {
        return Results.NotFound(new { error = "Poll not found" });
    }

    var response = new GetPollResponse
    {
        Question = poll.Question,
        RoomCode = poll.RoomCode,
        IsActive = poll.IsActive,
        Options = poll.Options.Select(o => new VoterPollOptionResponse
        {
            Id = o.Id,
            Text = o.Text
        }).ToList()
    };

    return Results.Ok(response);
})
.WithName("GetPoll")
.WithDescription("Get a poll by room code for voters");

app.MapPost("/api/polls/{roomCode}/vote", async (string roomCode, VoteRequest request, IPollRepository repository, IHubContext<PollHub> hubContext) =>
{
    var poll = repository.GetByRoomCode(roomCode);
    if (poll == null)
    {
        return Results.NotFound(new { error = "Poll not found" });
    }

    if (!poll.IsActive)
    {
        return Results.StatusCode(410);
    }

    var option = poll.Options.FirstOrDefault(o => o.Id == request.OptionId);
    if (option == null)
    {
        return Results.BadRequest(new { error = "Invalid option ID" });
    }

    if (!string.IsNullOrWhiteSpace(request.VoterId) && repository.HasVoterVoted(roomCode, request.VoterId))
    {
        return Results.Conflict(new { error = "You have already voted in this poll" });
    }

    var success = repository.AddVote(roomCode, request.OptionId, request.VoterId);
    if (!success)
    {
        return Results.BadRequest(new { error = "Unable to record vote" });
    }

    var updatedPoll = repository.GetByRoomCode(roomCode);
    var payload = new VoteUpdatePayload
    {
        RoomCode = roomCode,
        Results = updatedPoll!.Options.Select(o => new OptionVoteCount
        {
            OptionId = o.Id,
            Text = o.Text,
            VoteCount = o.VoteCount
        }).ToList()
    };

    await hubContext.Clients.Group(roomCode).SendAsync("VoteUpdated", payload);

    return Results.Ok(new { message = "Vote recorded successfully" });
})
.WithName("CastVote")
.WithDescription("Cast a vote for a poll option");

app.MapPost("/api/polls/{roomCode}/close", async (string roomCode, IPollRepository repository, IHubContext<PollHub> hubContext) =>
{
    var poll = repository.GetByRoomCode(roomCode);
    if (poll == null)
    {
        return Results.NotFound(new { error = "Poll not found" });
    }

    if (!poll.IsActive)
    {
        return Results.Ok(new { message = "Poll is already closed" });
    }

    var success = repository.ClosePoll(roomCode);
    if (!success)
    {
        return Results.BadRequest(new { error = "Unable to close poll" });
    }

    await hubContext.Clients.Group(roomCode).SendAsync("PollClosed", new { roomCode });

    return Results.Ok(new { message = "Poll closed successfully" });
})
.WithName("ClosePoll")
.WithDescription("Close a poll to prevent further voting");

app.Run();

public partial class Program { }
