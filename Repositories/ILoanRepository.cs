using Microsoft.AspNetCore.Mvc;
using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public interface ILoanRepository
    {
        Task<(List<Loan> List, int Count)> GetLoansAsync(int limit, int offset, LoanFilterDTO filter);

        Task<Loan> GetLoanAsync(int id);

        Task<LoanUpdateDTO> PutLoan(int id, List<string> fieldsToUpdate, LoanUpdateDTO updates, Loan entityToUpdate);
        Task<Loan> PostLoanAsync(LoanRequestDTO loanDTO);
        Task<Loan> DeleteLoanAsync(int id);
    }
}
