namespace TiTools_backend.Models
{
    public class Loan
    {
        public int LoanId { get; set; }
        public string? ApplicantName { get; set; }

        public DateTime RequestTime { get; set; }
        public DateTime ReturnTime { get; set; }

        public ICollection<Equipment>? Equipments { get; set; }

    }
}
