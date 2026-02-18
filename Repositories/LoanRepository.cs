using Microsoft.EntityFrameworkCore;
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

        public async Task<(List<Loan> List, int Count)> GetLoansAsync(
            int limit,
            int offset,
            LoanFilterDTO filter)
        {
            var query = _context.Loans.AsQueryable();

            //filter
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

            var count = await query.CountAsync();

            var list = await query
                    .Skip(offset)
                    .Take(limit)
                    .Include(l => l.Equipments)
                    .ToListAsync();

            return (list, count);
        }

        public async Task<Loan> GetLoanAsync(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Equipments)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (loan == null)
                throw new InvalidOperationException("Loan not found!");

            return loan;
        }

        public async Task<LoanUpdateDTO> PutLoan(int id, List<string> fieldsToUpdate, LoanUpdateDTO updates, Loan entityToUpdate)
        {
            foreach (var field in fieldsToUpdate)
            {
                _context
                    .Entry(entityToUpdate)
                    .Property(field).CurrentValue = typeof(LoanUpdateDTO)
                    .GetProperty(field)?
                    .GetValue(updates);
                _context
                    .Entry(entityToUpdate)
                    .Property(field).IsModified = true;
            }

            await _context.SaveChangesAsync();

            return updates;
        }

        public async Task<Loan> PostLoanAsync(LoanRequestDTO loanDTO)
        {
            var loan = new Loan
            {
                ApplicantName = loanDTO.ApplicantName,
                AuthorizedBy = loanDTO.AuthorizedBy,
                RequestTime = loanDTO.RequestTime,
                ReturnTime = loanDTO.ReturnTime,
                LoanStatus = true,
                Equipments = await _context.Equipments
                    .Where(e => loanDTO.EquipmentIds
                        .Contains(e.EquipmentId))
                .ToListAsync(),
            };


            await _context.AddAsync(loan);
            await _context.SaveChangesAsync();

            return loan;
        }
    }
}
