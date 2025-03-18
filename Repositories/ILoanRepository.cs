using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public interface ILoanRepository
    {
        public IQueryable<Loan> GetLoansFiltered(LoanFilterDTO filter);
    }
}
