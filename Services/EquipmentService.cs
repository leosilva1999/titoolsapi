using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.Context;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;

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
    }
}
