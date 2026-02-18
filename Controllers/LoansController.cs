using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using TiTools_backend.Services;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILoanService _loanService;

        public LoansController(AppDbContext context, ILoanRepository loanRepository,  ILoanService loanService)
        {
            _context = context;
            _loanService = loanService;
        }

        [Authorize(policy: "UserOnly")]
        [HttpGet]
        public async Task<ActionResult> GetLoansAsync(int limit, int offset, [FromQuery] LoanFilterDTO filter)
        {
            try
            {
                var (loanList, loanCount) = await _loanService.GetLoansAsync(limit, offset, filter);
                    return Ok(new
                    {
                        LoanList = loanList,
                        LoanCount = loanCount,
                        Errors = false
                    });           
            }
            catch (InvalidOperationException ioex)
            {
                return BadRequest(new { message = ioex.Message });
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: ex.Message,
                   statusCode: StatusCodes.Status500InternalServerError
                   );
            }
        }

        [Authorize(policy: "UserOnly")]
        [HttpGet("{id}", Name = "GetLoanAsync")]
        public async Task<ActionResult> GetLoanAsync(int id)
        {
            try
            {
                var loan = await _loanService.GetLoanAsync(id);

                return Ok(loan);
            }
            catch (InvalidOperationException ioex)
            {
                return BadRequest(new { message = ioex.Message });
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: ex.Message,
                   statusCode: StatusCodes.Status500InternalServerError
                   );
            }

        }

        [Authorize(policy: "UserOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoan(int id, [FromBody] LoanUpdateDTO updates)
        {
            try
            {
                var result = await _loanService.PutLoan(id, updates);

                return NoContent();
            }
            catch (InvalidOperationException ioex)
            {
                return BadRequest(new { message = ioex.Message });
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: ex.Message,
                   statusCode: StatusCodes.Status500InternalServerError
                   );
            }
        }

        [Authorize(policy: "UserOnly")]
        [HttpPost]
        public async Task<ActionResult<Loan>> PostLoan([FromBody] LoanRequestDTO loanDTO)
        {
            try
            {
                var result = await _loanService.PostLoan(loanDTO);


                return CreatedAtRoute("GetLoanAsync", new { id = result.LoanId }, new Response
                {
                    Status = "Created",
                    Message = $"Loan created Successfuly",
                    Return = result
                });
            }
            catch (InvalidOperationException ioex)
            {
                return BadRequest(new { message = ioex.Message });
            }
            catch (Exception ex)
            {
                return Problem(
                   detail: ex.Message,
                   statusCode: StatusCodes.Status500InternalServerError
                   );
            }
        }

        [Authorize(policy: "UserOnly")]
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

        [Authorize(policy: "UserOnly")]
        [HttpPut("/api/Loans/deleteequipmentfromloan/{equipmentId}")]
        public async Task<IActionResult> DeleteEquipmentFromLoan(int equipmentId)
        {
            var activeLoansWithThisEquipment = await _context.Loans
                .Include(l => l.Equipments)
                .Where(l => l.LoanStatus == true && l.Equipments.Any(e => e.EquipmentId == equipmentId))
                .FirstOrDefaultAsync();

            var equipmentToRemove = await _context.Equipments.FindAsync(equipmentId);

            try
            {
                if (activeLoansWithThisEquipment != null && equipmentToRemove != null)
                {
                    activeLoansWithThisEquipment.Equipments.Remove(equipmentToRemove);
                    await _context.SaveChangesAsync();
                    var teste = await _context.Loans
                    .Where(l => l.LoanStatus == true && l.Equipments.Any(e => e.EquipmentId == equipmentId))
                    .ToListAsync();
                    Console.WriteLine("nada");
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
