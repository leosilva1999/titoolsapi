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
        public async Task<IActionResult> DeleteLoanAsync(int id)
        {
            try
            {
                var response = await _loanService.DeleteLoanAsync(id);

                return Ok(new Response
                {
                    Return = response,
                    Status = "OK",
                    Message = "Loan deleted"
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


        //REFATORAR
        [Authorize(policy: "UserOnly")]
        [HttpPut("/api/Loans/deleteequipmentfromloan/{equipmentId}")]
        public async Task<IActionResult> DeleteEquipmentFromLoan(int equipmentId)
        {


            try
            {
                var response = await _loanService.DeleteEquipmentFromLoan(equipmentId);

                return Ok(new Response
                {
                    Return = response,
                    Status = "OK",
                    Message = "Equipment deleted From Loan"
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
    }
}
