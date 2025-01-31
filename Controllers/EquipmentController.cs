﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> GetEquipments(int limit, int offset)
        {
            
                var equipmentList = await _context.Equipments
                    .OrderBy(x => x.EquipmentName)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
                var equipmentCount = await _context.Equipments.CountAsync();


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
        public async Task<IActionResult> GetEquipment(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);

            if(equipment is null)
            {
                return BadRequest();
            }
            return Ok(equipment);
        }

        [Authorize(policy: "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PostEquipment(Equipment model)
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
                return StatusCode(StatusCodes.Status201Created,
                    new Response
                    {
                        Status = "Created",
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
                        Message = $"Equipment creation failed: {ex}"
                    });
            }


        }

        [Authorize(Policy = "UserOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipment(int id, Equipment model) {

            if (id != model.EquipmentId)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found1"
                    });
            }

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }catch (Exception ex)
            {
                if (!EquipmentExists(id)){
                    return StatusCode(StatusCodes.Status404NotFound,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found"
                    });
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [Authorize(Policy = "UserOnly")]
        [HttpPut("/api/Equipment/updatestatus/{id}")]
        public async Task<IActionResult> UpdateStatusEquipment(int id, bool equipmentStatus) 
        {
            var equipmentExists = await _context.Equipments.FindAsync(id);
            if (equipmentExists == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found"
                    });
            }

            equipmentExists.EquipmentLoanStatus = equipmentStatus;
            try
            {
                await _context.SaveChangesAsync();
            }catch (Exception ex)
            {
                if (!EquipmentExists(id))
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found"
                    });
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [Authorize(Policy = "UserOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);

            if (equipment is null)
            {
                return BadRequest();
            }

            _context.Equipments.Remove(equipment);

            try
            {
                await _context.SaveChangesAsync();
            }catch(Exception ex)
            {
                if (!EquipmentExists(id))
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found"
                    });
                }
                else
                {
                    throw;
                }
            }
            return Ok(new
            {
                Equipment = equipment,
                Status = "OK",
                Message = "Equipment deleted"
            });
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.EquipmentId == id);
        }
    }
}
