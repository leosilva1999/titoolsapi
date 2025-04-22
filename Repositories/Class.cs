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

            if (filter.RequestTimeMin.HasValue)
                query = query.Where(p => p.RequestTime >= filter.RequestTimeMin);

            if (filter.RequestTimeMax.HasValue)
                query = query.Where(p => p.RequestTime <= filter.RequestTimeMax);

            if (filter.ReturnTimeMin.HasValue)
                query = query.Where(p => p.ReturnTime >= filter.ReturnTimeMin);

            if (filter.ReturnTimeMax.HasValue)
                query = query.Where(p => p.ReturnTime <= filter.ReturnTimeMax);

            if (filter.LoanStatus.HasValue)
                query = query.Where(p => p.LoanStatus == filter.LoanStatus.Value);

            if (filter.OrderByDescending)
                query = query.OrderByDescending(x => x.RequestTime);

            return query;
        }
    }
}
