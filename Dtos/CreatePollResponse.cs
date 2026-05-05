namespace _1_the_real_time_polling_app_focus_signalr_websockets.Dtos;

public class CreatePollResponse
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public List<PollOptionResponse> Options { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PollOptionResponse
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
}
