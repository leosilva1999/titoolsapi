using System.ComponentModel.DataAnnotations;

namespace TiTools_backend.DTOs
{
    public class RegisterModelDTO
    {
        [Required(ErrorMessage = "User name is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "E-mail is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
