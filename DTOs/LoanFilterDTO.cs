using System.ComponentModel.DataAnnotations;

namespace TiTools_backend.DTOs
{
    public class LoanFilterDTO
    {
        [Required(ErrorMessage = "O nome do solicitante é obrigatório", AllowEmptyStrings = false)]
        [StringLength((50), ErrorMessage = "O nome pode possuir no máximo 50 caracteres...")]
        public string? ApplicantName { get; set; }
        public string? AuthorizedBy { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ReturnTime { get; set; }
        public bool LoanStatus { get; set; }
        public bool OrderByDescending { get; set; }
    }
}
