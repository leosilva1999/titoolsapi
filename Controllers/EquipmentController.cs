﻿using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentController(AppDbContext context, IEquipmentRepository equipmentRepository)
        {
            _context = context;
            _equipmentRepository = equipmentRepository;
        }

        //get

        
        [Authorize(Policy = "UserOnly")]
        [HttpGet]
        public async Task<IActionResult> GetEquipments(int limit, int offset, [FromQuery] EquipmentFilterDTO filter)
        {
            var filteredQuery = _equipmentRepository.GetEquipmentsFiltered(filter);

            var equipmentCount = await filteredQuery.CountAsync();

            var equipmentList = await filteredQuery
                    .OrderBy(x => x.EquipmentName)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
               


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
        public async Task<IActionResult> PutEquipment(int id, [FromBody] EquipmentUpdateDTO updates) {
            var entityToUpdate = await _context.Equipments
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (entityToUpdate == null) return NotFound();

            var fieldsToUpdate = new List<string>();

            if(updates.EquipmentName != null) fieldsToUpdate.Add("EquipmentName");
            if(updates.IpAddress != null) fieldsToUpdate.Add("IpAddress");
            if(updates.MacAddress != null) fieldsToUpdate.Add("MacAddress");
            if(updates.EquipmentLoanStatus != null) fieldsToUpdate.Add("EquipmentLoanStatus");

            foreach (var field in fieldsToUpdate)
            {
                _context
                    .Entry(entityToUpdate)
                    .Property(field).CurrentValue = typeof(EquipmentUpdateDTO)
                    .GetProperty(field)?
                    .GetValue(updates);
                _context
                    .Entry(entityToUpdate)
                    .Property(field).IsModified = true;
            }

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
        [HttpPut("/api/Equipment/updatestatus")]
        public async Task<IActionResult> UpdateStatusEquipment(List<int> EquipmentIds, bool equipmentStatus) 
        {
            var equipments = await _context.Equipments
                .Where(e => EquipmentIds
                .Contains(e.EquipmentId))
                .ToListAsync();

            if (equipments.Count != EquipmentIds.Count)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = "One or more Equipments don't exist"
                    });
            }

            foreach(var equipment in equipments)
            {
                equipment.EquipmentLoanStatus = equipmentStatus;
            }
            try
            {
                await _context.SaveChangesAsync();
            }catch (Exception ex)
            {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "Failed to update equipment status: " + ex.Message
                    });
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
