using System.ComponentModel.DataAnnotations;

namespace CommonObjects.Requests;

public class UserAuthorizeRequest
{
    [Required, MinLength(2), MaxLength(50)]
    public string Login { get; set; } = null!;
    [Required, MinLength(2), MaxLength(50)]
    public string Password { get; set; } = null!;
}
