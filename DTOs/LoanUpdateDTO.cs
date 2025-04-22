using System.ComponentModel.DataAnnotations;

namespace TiTools_backend.DTOs
{
    public class LoanUpdateDTO
    {
        public string? ApplicantName { get; set; }
        public string? AuthorizedBy { get; set; }

        public DateTime? RequestTime { get; set; }
        public DateTime? ReturnTime { get; set; }
        public bool? LoanStatus { get; set; }

        public List<int>? EquipmentIds { get; set; } = [];
    }
}
