using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Services
{
    public interface IEquipmentService
    {
        Task<(IEnumerable<Equipment> List, int Count)> GetEquipmentsAsync(int limit, int offset, EquipmentFilterDTO filter);
        Task<IEnumerable<object>> GetEquipmentWithLoans(int id);
    }
}
