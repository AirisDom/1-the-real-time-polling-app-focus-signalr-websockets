using _1_the_real_time_polling_app_focus_signalr_websockets.Repositories;

namespace _1_the_real_time_polling_app_focus_signalr_websockets.Services;

public interface IRoomCodeGenerator
{
    string GenerateUniqueCode();
}

public class RoomCodeGenerator : IRoomCodeGenerator
{
    private readonly IPollRepository _pollRepository;
    private readonly Random _random = new();
    private const int MaxAttempts = 100;

    public RoomCodeGenerator(IPollRepository pollRepository)
    {
        _pollRepository = pollRepository;
    }

    public string GenerateUniqueCode()
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var code = _random.Next(0, 10000).ToString("D4");

            if (_pollRepository.GetByRoomCode(code) == null)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique room code after maximum attempts.");
    }
}
