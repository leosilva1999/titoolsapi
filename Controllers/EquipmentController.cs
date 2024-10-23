using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentController(AppDbContext context)
        {
            _context = context;
        }

        //get

        
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
                var equipmentList = await _context.Equipments.ToListAsync();


            if (equipmentList is not null)
            {
                return Ok(equipmentList);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEquipment([FromBody] Equipment model)
        {
            var equipmentExists = await _context.Equipments.FindAsync(model.EquipmentId);

            if (equipmentExists is not null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment already exists!"
                    });
            }

            Equipment equipment = new()
            {
                EquipmentName = model.EquipmentName,
                IpAddress = model.IpAddress,
                MacAddress = model.MacAddress,
                QrCode = model.QrCode,
                EquipmentLoanStatus = model.EquipmentLoanStatus,
            };


            try
            {
                await _context.AddAsync(equipment);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK,
                    new Response
                    {
                        Status = "Success",
                        Message = $"Equipment {model.EquipmentName} created Successfuly"
                    });
            }
            catch (Exception ex)
            {

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = $"User creation failed: {ex}"
                    });
            }


        }
    }
}
