using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TiTools_backend.Models
{
    [Table("Equipments")]
    public class Equipment
    {
        [Key]   
        [Required]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "O nome do equipamento é obrigatório", AllowEmptyStrings = false)]
        [StringLength((50), ErrorMessage = "O nome pode possuir no máximo 50 caracteres...")]
        public string? EquipmentName { get; set; }
  
        public string? IpAddress { get; set; }
      
        public string? MacAddress { get; set; }

        public string? QrCode { get; set; }

        [Required(ErrorMessage = "O status do empréstimo do equipamento é obrigatório", AllowEmptyStrings = false)]
        public bool EquipmentLoanStatus { get; set; }

        public List<Loan> Loans { get; set; } = [];
    }
}
