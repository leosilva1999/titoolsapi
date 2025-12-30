using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiTools_backend.DTOs;
using TiTools_backend.Services;
using TiTools_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _equipmentService;

        public EquipmentController(IEquipmentService equipmentService)
        {
            _equipmentService = equipmentService;
        }

        //get

        [HttpGet("/api/Equipment/getequipment/{id}", Name = "GetEquipmentById")]
        public async Task<IActionResult> GetEquipmentAsync(int id)
        {
            var equipment = await _equipmentService.GetEquipmentAsync(id);

            if (equipment is not null)
            {
                return Ok(equipment);
            }
            return BadRequest(new { errors = "400", message = "Falha na requisição" });
        }

        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetEquipmentsAsync(int limit, int offset, [FromQuery] EquipmentFilterDTO filter)
        {
            var (equipmentList, equipmentCount) = await _equipmentService.GetEquipmentsAsync(limit, offset, filter);




            if (equipmentList is not null)
            {
                return Ok(new
                {
                    EquipmentList = equipmentList,
                    EquipmentCount = equipmentCount,
                    Errors = false
                });
            }
            return BadRequest(new { errors = "400", message = "Falha na requisição" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEquipmentWithLoansAsync(int id)
        {
            try
            {
                var equipment = await _equipmentService.GetEquipmentWithLoansAsync(id);

                return Ok(equipment);
            }
            catch(InvalidOperationException ioex)
            {
                    return BadRequest(new { message = ioex.Message });
            }
            catch (Exception ex) {
                return Problem(
                   detail: ex.Message,
                   statusCode: StatusCodes.Status500InternalServerError
                   );
            }
        }

        [Authorize(policy: "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PostEquipmentAsync(Equipment model)
        {
            try
            {
                var result = await _equipmentService.PostEquipmentAsync(model);

                return CreatedAtRoute("GetEquipmentById", new { id = result.EquipmentId }, new Response
                {
                    Status = "Created",
                    Message = $"Equipment {model.EquipmentName} created Successfuly",
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

        [Authorize(Policy = "UserOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipmentAsync(int id, [FromBody] EquipmentUpdateDTO updates) {
            try
            {
                var result = await _equipmentService.PutEquipmentAsync(id, updates);

                return NoContent();
            }
            catch (InvalidOperationException ioex)
            {
                return BadRequest(new { message = ioex.Message });
            }
            catch (KeyNotFoundException knfex)
            {
                return BadRequest(new { message = knfex.Message });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                    );
            }
        }

        [Authorize(Policy = "UserOnly")]
        [HttpPut("/api/Equipment/updatestatus/{equipmentStatus}")]
        public async Task<IActionResult> UpdateStatusEquipmentAsync(List<int> EquipmentIds, bool equipmentStatus) 
        {
            try
            {
                await _equipmentService.UpdateStatusEquipmentAsync(EquipmentIds, equipmentStatus);

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

        [Authorize(Policy = "UserOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentAsync(int id)
        {
            try
            {
                var response = await _equipmentService.DeleteEquipmentAsync(id);

                return Ok(new Response
                {
                    Return = response,
                    Status = "OK",
                    Message = "Equipment deleted"
                });
            }
            catch(InvalidOperationException ioex)
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
