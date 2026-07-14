using System.ComponentModel.DataAnnotations;

namespace MyPanelExpress.Models;

public class UserLogin
{
    [Required(ErrorMessage = "L'email est obligatoire")]
    [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est obligatoire")]
    public string Password { get; set; } = string.Empty;
}

public class UserDetails
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UserDetails() { }

    public UserDetails(UserApp user)
    {
        Id = user.Id;
        Email = user.Email ?? string.Empty;
        FirstName = user.FirstName;
        LastName = user.LastName;
        CreatedAt = user.CreatedAt;
    }
}

public class LoginResponse
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    public UserDetails? User { get; set; }
}
