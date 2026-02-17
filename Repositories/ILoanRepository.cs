using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public interface ILoanRepository
    {
        Task<(List<Loan> List, int Count)> GetLoansAsync(int limit, int offset, LoanFilterDTO filter);


    }
}
