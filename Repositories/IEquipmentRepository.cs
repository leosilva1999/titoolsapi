using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public interface IEquipmentRepository
    {
        public IQueryable<Equipment> GetEquipmentsFiltered(EquipmentFilterDTO filter);
    }
}
