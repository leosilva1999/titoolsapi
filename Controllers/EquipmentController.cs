using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Services;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

        
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetEquipments(int limit, int offset, [FromQuery] EquipmentFilterDTO filter)
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
        public async Task<IActionResult> GetEquipmentWithLoans(int id)
        {
            var equipment = await _equipmentService.GetEquipmentWithLoans(id);

            if (equipment is null)
            {
                return BadRequest();
            }
            return Ok(equipment);
        }

        [Authorize(policy: "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PostEquipment(Equipment model)
        {
            try
            {
                var result = await _equipmentService.PostEquipment(model);

                return StatusCode(
                    StatusCodes.Status201Created, new Response
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
        public async Task<IActionResult> PutEquipment(int id, [FromBody] EquipmentUpdateDTO updates) {
            try
            {
                var result = await _equipmentService.PutEquipment(id, updates);

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
        public async Task<IActionResult> UpdateStatusEquipment(List<int> EquipmentIds, bool equipmentStatus) 
        {
            try
            {
                await _equipmentService.UpdateStatusEquipment(EquipmentIds, equipmentStatus);

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
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            try
            {
                var response = await _equipmentService.DeleteEquipment(id);

                return Ok(new
                {
                    Equipment = response,
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
