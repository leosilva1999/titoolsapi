using System.ComponentModel.DataAnnotations;

namespace TiTools_backend.DTOs
{
    public class LoanFilterDTO
    {
        public string? ApplicantName { get; set; }
        public string? AuthorizedBy { get; set; }
        public DateTime? RequestTimeMin { get; set; }
        public DateTime? RequestTimeMax { get; set; }
        public DateTime? ReturnTimeMin { get; set; }
        public DateTime? ReturnTimeMax { get; set; }
        public bool? LoanStatus { get; set; }
        public bool OrderByDescending { get; set; }
    }
}
