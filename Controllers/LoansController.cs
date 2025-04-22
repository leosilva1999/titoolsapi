using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILoanRepository _loanRepository;

        public LoansController(AppDbContext context, ILoanRepository loanRepository)
        {
            _context = context;
            _loanRepository = loanRepository;
        }

        // GET: api/Loans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans(int limit, int offset, [FromQuery] LoanFilterDTO filter)
        {
            /*var loanList = await _context.Loans
                    .OrderByDescending(x => x.RequestTime)
                    .Skip(offset)

                    .Take(limit)
                    .ToListAsync();*/
            var filteredQuery = _loanRepository.GetLoansFiltered(filter);
            
            var loanCount = await filteredQuery.CountAsync();

            var loanList = filteredQuery
                    .Skip(offset)
                    .Take(limit)
                    .Include(l => l.Equipments)
                    .ToListAsync();

            if (loanList is not null)
            {
                return Ok(new
                {
                    LoanList = loanList,
                    LoanCount = loanCount,
                    Errors = false
                });
            }
            return BadRequest(new { errors = "400", message = "Falha na requisição" });
        }

        // GET: api/Loans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            return loan;
        }

        // PUT: api/Loans/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoan(int id, Loan loan)
        {
            if (id != loan.LoanId)
            {
                return BadRequest();
            }

            _context.Entry(loan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Loans
        [HttpPost]
        public async Task<ActionResult<Loan>> PostLoan([FromBody] LoanRequestDTO loanDTO)
        {
            Console.WriteLine(loanDTO);
            var loan = new Loan
            {
                ApplicantName = loanDTO.ApplicantName,
                AuthorizedBy = loanDTO.AuthorizedBy,
                RequestTime = loanDTO.RequestTime,
                ReturnTime = loanDTO.ReturnTime,
                LoanStatus = true,
                Equipments =  await _context.Equipments
                    .Where(e => loanDTO.EquipmentIds
                        .Contains(e.EquipmentId))
                    .ToListAsync(),
            };

            try
            {
                await _context.AddAsync(loan);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status201Created,
                    new Response
                    {
                        Status = "Created",
                        Message = $"Loan created Successfuly"
                    });
            }
            catch (Exception ex)
            {

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = $"Loan creation failed: {ex}"
                    });
            }
        }

        // DELETE: api/Loans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("/api/Loans/deleteequipmentfromloan/{id}")]
        public async Task<IActionResult> DeleteEquipmentFromLoan(int id)
        {
            var activeLoansWithThisEquipment = await _context.Loans
                .Where(l => l.LoanStatus == true && l.Equipments
                .Any(e => e.EquipmentId == id))
                .FirstOrDefaultAsync();

            var equipmentToRemove = await _context.Equipments.FindAsync(id);

            try
            {
                if (activeLoansWithThisEquipment != null && equipmentToRemove != null)
                {
                    activeLoansWithThisEquipment.Equipments.Remove(equipmentToRemove);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = $"Erro ao remover o equipamento"
                    });
                }
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = $"Erro ao remover o equipamento: {ex}"
                    });
            }
        }

        private bool LoanExists(int id)
        {
            return _context.Loans.Any(e => e.LoanId == id);
        }
    }
}
