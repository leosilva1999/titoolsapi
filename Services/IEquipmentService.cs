using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Services
{
    public interface IEquipmentService
    {
        Task<Equipment> GetEquipmentAsync(int id);
        Task<(IEnumerable<Equipment> List, int Count)> GetEquipmentsAsync(int limit, int offset, EquipmentFilterDTO filter);
        Task<IEnumerable<object>> GetEquipmentWithLoansAsync(int id);
        Task<Equipment> PostEquipmentAsync(Equipment model);
        Task<EquipmentUpdateDTO> PutEquipmentAsync(int id, EquipmentUpdateDTO updates);
        Task<IEnumerable<Equipment>> UpdateStatusEquipmentAsync(List<int> EquipmentIds, bool equipmentStatus);
        Task<Equipment> DeleteEquipmentAsync(int id);

    }
}
