using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace TiTools_backend.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly AppDbContext _context;
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentService(AppDbContext context, IEquipmentRepository equipmentRepository)
        {
            _context = context;
            _equipmentRepository = equipmentRepository;
        }

        public async Task<(IEnumerable<Equipment> List, int Count)> GetEquipmentsAsync(int limit, int offset, EquipmentFilterDTO filter)
        {
            var filteredQuery = _equipmentRepository.GetEquipmentsFiltered(filter);

            var equipmentCount = await filteredQuery.CountAsync();

            var equipmentList = await filteredQuery
                    .OrderBy(x => x.EquipmentName)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

            return (equipmentList, equipmentCount);
        }

        public async Task<IEnumerable<object>> GetEquipmentWithLoans(int id)
        {
            var equipment = await _context.Equipments
                .Where(e => e.EquipmentId == id)
                .Select(e => new
                {
                    e.EquipmentId,
                    e.EquipmentName,
                    Loans = e.Loans.OrderByDescending(l => l.RequestTime).Select(l => new { l.ApplicantName, l.RequestTime, l.ReturnTime, l.LoanStatus })
                })
                .ToListAsync();
            return equipment ;
        }

        public async Task<Equipment> PostEquipment(Equipment model)
        {
            try
            {
                var equipmentExists = await _context.Equipments.FindAsync(model.EquipmentId);

                if (equipmentExists is not null)
                {
                    throw new InvalidOperationException("Equipment already exists!");
                }

                Equipment equipment = new()
                {
                    EquipmentName = model.EquipmentName,
                    IpAddress = model.IpAddress,
                    MacAddress = model.MacAddress,
                    QrCode = model.QrCode,
                    EquipmentLoanStatus = model.EquipmentLoanStatus,
                };
                
                await _context.AddAsync(equipment);
                await _context.SaveChangesAsync();

                return equipment;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<EquipmentUpdateDTO> PutEquipment(int id, EquipmentUpdateDTO updates)
        {
            try
            {
                var entityToUpdate = await _context.Equipments
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

                if (entityToUpdate == null) throw new InvalidOperationException("Equipment not found!"); ;

                var fieldsToUpdate = new List<string>();

                if (updates.EquipmentName != null) fieldsToUpdate.Add("EquipmentName");
                if (updates.IpAddress != null) fieldsToUpdate.Add("IpAddress");
                if (updates.MacAddress != null) fieldsToUpdate.Add("MacAddress");
                if (updates.EquipmentLoanStatus != null) fieldsToUpdate.Add("EquipmentLoanStatus");

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

                    await _context.SaveChangesAsync();

                    if (!EquipmentExists(id))
                    {
                        throw new KeyNotFoundException("Equipment not found");
                    }

                return updates;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Equipment>> UpdateStatusEquipment(List<int> EquipmentIds, bool equipmentStatus)
        {
            try
            {
                var equipments = await _context.Equipments
                .Where(e => EquipmentIds
                .Contains(e.EquipmentId))
                .ToListAsync();

                if (equipments.Count != EquipmentIds.Count) 
                    throw new InvalidOperationException("One or more Equipments don't exist");

                foreach (var equipment in equipments)
                {
                    equipment.EquipmentLoanStatus = equipmentStatus;
                }
                
                await _context.SaveChangesAsync();
                
                return equipments;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.EquipmentId == id);
        }
    }
}
