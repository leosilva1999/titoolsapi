using System.ComponentModel.DataAnnotations;

namespace TiTools_backend.DTOs
{
    public class LoginModelDTO
    {
        [EmailAddress]
        [Required(ErrorMessage ="E-mail is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
