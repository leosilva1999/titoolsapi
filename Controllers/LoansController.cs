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
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanService _loanService;

        public LoansController(AppDbContext context, ILoanRepository loanRepository,  ILoanService loanService)
        {
            _context = context;
            _loanRepository = loanRepository;
            _loanService = loanService;
        }

        [Authorize(policy: "UserOnly")]
        [HttpGet]
        public async Task<ActionResult> GetLoans(int limit, int offset, [FromQuery] LoanFilterDTO filter)
        {
            var (loanList, loanCount) = await _loanService.GetLoansAsync(limit, offset, filter);

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

        [Authorize(policy: "UserOnly")]
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

        [Authorize(policy: "UserOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoan(int id, [FromBody] LoanUpdateDTO updates)
        {
            var entityToUpdate = await _context.Loans
                .Include(l => l.Equipments)
                .FirstOrDefaultAsync(l => l.LoanId == id);

            if (entityToUpdate == null) return NotFound();
            if (updates.ReturnTime < updates.RequestTime)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = $"A data de recebimento é menor que a data de empréstimo"
                    });
            }

            var fieldsToUpdate = new List<string>();

            if (updates.LoanStatus != null) fieldsToUpdate.Add("LoanStatus");
            if (updates.ApplicantName != null) fieldsToUpdate.Add("ApplicantName");
            if (updates.AuthorizedBy != null) fieldsToUpdate.Add("AuthorizedBy");
            if (updates.RequestTime != null) fieldsToUpdate.Add("RequestTime");
            if (updates.ReturnTime != null) fieldsToUpdate.Add("ReturnTime");

            if (updates.EquipmentIds != null && updates.EquipmentIds.Any()) {
                var existingEquipments = entityToUpdate.Equipments
                    .ToList();

                var newEquipments = await _context.Equipments
                    .Where(e => updates.EquipmentIds
                        .Contains(e.EquipmentId))
                    .ToListAsync();

                foreach(var equipment in existingEquipments)
                {
                    if(!newEquipments.Any(e => e.EquipmentId == equipment.EquipmentId))
                    {
                        entityToUpdate.Equipments.Remove(equipment);
                    }
                }

                foreach(var equipment in newEquipments)
                {
                    if(!existingEquipments.Any(e => e.EquipmentId == equipment.EquipmentId)){
                        entityToUpdate.Equipments.Add(equipment);
                    }
                }
                
            }

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

        [Authorize(policy: "UserOnly")]
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

            if (loan.ReturnTime < loan.RequestTime)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = $"A data de recebimento é menor que a data de empréstimo"
                    });
            }

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
