using System.ComponentModel.DataAnnotations;

namespace _1_the_real_time_polling_app_focus_signalr_websockets.Dtos;

public class CreatePollRequest
{
    [Required(ErrorMessage = "Question is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Question must be between 1 and 500 characters")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Options are required")]
    [MinLength(2, ErrorMessage = "At least 2 options are required")]
    [MaxLength(4, ErrorMessage = "Maximum 4 options are allowed")]
    public List<string> Options { get; set; } = new();
}
