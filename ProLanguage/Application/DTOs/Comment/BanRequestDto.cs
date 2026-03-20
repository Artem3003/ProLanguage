using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment;

public class BanRequestDto
{
    [Required(ErrorMessage = "User name is required")]
    public string User { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration is required")]
    public string Duration { get; set; } = string.Empty;
}
