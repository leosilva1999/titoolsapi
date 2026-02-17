using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly AppDbContext _context;

        public EquipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        
        public async Task<Equipment> GetEquipmentAsync(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);

            if (equipment == null)
                throw new InvalidOperationException("Equipment not found!");

            return equipment;
        }

        public async Task<(List<Equipment> List, int Count)> GetEquipmentsAsync(
            int limit,
            int offset,
            EquipmentFilterDTO filter)
        {
            var query = _context.Equipments.AsQueryable();

            //filter
            if (!string.IsNullOrEmpty(filter.EquipmentName))
                query = query.Where(p => p.EquipmentName.Contains(filter.EquipmentName));
            if (!string.IsNullOrEmpty(filter.MacAddress))
                query = query.Where(p => p.MacAddress.Contains(filter.MacAddress));
            if (filter.EquipmentLoanStatus.HasValue)
                query = query.Where(p => p.EquipmentLoanStatus == filter.EquipmentLoanStatus);
            if (!string.IsNullOrEmpty(filter.Type))
                query = query.Where(p => p.Type.Contains(filter.Type)); 
            if (!string.IsNullOrEmpty(filter.Manufacturer))
                query = query.Where(p => p.Manufacturer.Contains(filter.Manufacturer)); 
            if (!string.IsNullOrEmpty(filter.Model))
                query = query.Where(p => p.Model.Contains(filter.Model));

            var count = await query.CountAsync();

            var list = await query
                    .OrderBy(x => x.EquipmentName)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();
            return (list, count);
        }

        public async Task<IEnumerable<object>> GetEquipmentWithLoansAsync(int id)
        {
            var equipment = await _context.Equipments
                    .Where(e => e.EquipmentId == id)
                    .Select(e => new
                    {
                        e.EquipmentId,
                        e.EquipmentName,
                        e.Type,
                        e.Manufacturer,
                        e.Model,
                        e.EquipmentLoanStatus,
                        Loans = e.Loans.OrderByDescending(l => l.RequestTime).Select(l => new { l.ApplicantName, l.RequestTime, l.ReturnTime, l.LoanStatus })
                    })
                    .ToListAsync();

            if (equipment.Count() == 0)
                throw new InvalidOperationException("Equipment not found!");

            return equipment;
        }

        public async Task<Equipment> PostEquipmentAsync(Equipment model)
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
                Type = model.Type,
                Manufacturer = model.Manufacturer,
                Model = model.Model,
                EquipmentLoanStatus = model.EquipmentLoanStatus,
            };

            await _context.AddAsync(equipment);
            await _context.SaveChangesAsync();

            return equipment;
        }

        public async Task<EquipmentUpdateDTO> PutEquipmentAsync(int id, EquipmentUpdateDTO updates)
        {
            var entityToUpdate = await _context.Equipments
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (entityToUpdate == null) throw new InvalidOperationException("Equipment not found!"); ;

            var fieldsToUpdate = new List<string>();

            if (updates.EquipmentName != null) fieldsToUpdate.Add("EquipmentName");
            if (updates.IpAddress != null) fieldsToUpdate.Add("IpAddress");
            if (updates.MacAddress != null) fieldsToUpdate.Add("MacAddress");
            if (updates.Type != null) fieldsToUpdate.Add("Type");
            if (updates.Manufacturer != null) fieldsToUpdate.Add("Manufacturer");
            if (updates.Model != null) fieldsToUpdate.Add("Model");
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

        public async Task<IEnumerable<Equipment>> UpdateStatusEquipmentAsync(List<int> EquipmentIds, bool equipmentStatus)
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

        public async Task<Equipment> DeleteEquipmentAsync(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);

            if (equipment is null)
                throw new InvalidOperationException("Equipment not found");

            _context.Equipments.Remove(equipment);


            await _context.SaveChangesAsync();


            if (!EquipmentExists(id))
                throw new InvalidOperationException("Equipment not found");

            return equipment;
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipments.Any(e => e.EquipmentId == id);
        }
    }
}
