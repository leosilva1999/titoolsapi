using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiTools_backend.Models
{
    [Table("Loans")]
    public class Loan
    {
        [Key]
        [Required]
        public int LoanId { get; set; }

        [Required(ErrorMessage = "O nome do solicitante é obrigatório", AllowEmptyStrings = false)]
        [StringLength((50), ErrorMessage = "O nome pode possuir no máximo 50 caracteres...")]
        public string? ApplicantName { get; set; }

        public DateTime RequestTime { get; set; }
        public DateTime ReturnTime { get; set; }

        public ICollection<Equipment>? Equipments { get; set; }

    }
}
