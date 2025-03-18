using Microsoft.IdentityModel.Tokens;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;

        public LoanRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Loan> GetLoansFiltered(LoanFilterDTO filter)
        {
            var query = _context.Loans.AsQueryable();

            if (!string.IsNullOrEmpty(filter.ApplicantName))  
                query = query.Where(p => p.ApplicantName.Contains(filter.ApplicantName));
            
            if (!string.IsNullOrEmpty(filter.AuthorizedBy))
                query = query.Where(p => p.AuthorizedBy.Contains(filter.AuthorizedBy));

            if (filter.RequestTime.HasValue)
                query = query.Where(p => p.RequestTime >= filter.RequestTime);

            if (filter.ReturnTime.HasValue)
                query = query.Where(p => p.RequestTime >= filter.ReturnTime);

            if (filter.LoanStatus)
                query = query.Where(p => p.LoanStatus);

            return query;
        }
    }
}
