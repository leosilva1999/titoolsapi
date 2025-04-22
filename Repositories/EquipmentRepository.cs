using Microsoft.IdentityModel.Tokens;
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

        public IQueryable<Equipment> GetEquipmentsFiltered(EquipmentFilterDTO filter)
        {
            var query = _context.Equipments.AsQueryable();

            if (!string.IsNullOrEmpty(filter.EquipmentName))
                query = query.Where(p => p.EquipmentName.Contains(filter.EquipmentName));

            if (!string.IsNullOrEmpty(filter.MacAddress))
                query = query.Where(p => p.MacAddress.Contains(filter.MacAddress));

            if (filter.EquipmentLoanStatus.HasValue)
                query = query.Where(p => p.EquipmentLoanStatus == filter.EquipmentLoanStatus);

            return query;
        }
    }
}
