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
        public async Task<IActionResult> GetEquipments()
        {
            
                var equipmentList = await _context.Equipments.ToListAsync();


            if (equipmentList is not null)
            {
                return Ok(equipmentList);
            }
            return BadRequest();
        }

        [Authorize(policy: "UserOnly")]
        [HttpPost]
        public async Task<IActionResult> PostEquipment([FromBody] Equipment model)
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
                        Message = $"User creation failed: {ex}"
                    });
            }


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipment(int id, Equipment model)
        {

            if (id != model.EquipmentId)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = "Equipment not found"
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
        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.EquipmentId == id);
        }
    }
}
