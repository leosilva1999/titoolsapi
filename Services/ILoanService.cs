using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Services
{
    public interface ILoanService
    {
        Task<(IEnumerable<Loan> loanList, int loanCount)> GetLoansAsync(int limit, int offset, LoanFilterDTO filter);
        Task<Loan> GetLoanAsync(int id);
    }
}
