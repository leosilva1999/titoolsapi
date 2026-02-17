using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;

namespace TiTools_backend.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;

        public LoanService(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<(IEnumerable<Loan> loanList, int loanCount)> GetLoansAsync(int limit, int offset, LoanFilterDTO filter)
        {
            try
            {
                return await _loanRepository.GetLoansAsync(limit, offset, filter);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Loan> GetLoanAsync(int id)
        {
            try
            {
                return await _loanRepository.GetLoanAsync(id);
            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}
