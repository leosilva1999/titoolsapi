using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Repositories
{
    public interface IEquipmentRepository
    {

        Task<Equipment> GetEquipmentAsync(int id);

        Task<(List<Equipment> List, int Count)> GetEquipmentsAsync(
            int limit,
            int offset,
            EquipmentFilterDTO filter);

        Task<IEnumerable<object>> GetEquipmentWithLoansAsync(int id);

        Task<Equipment> PostEquipmentAsync(Equipment model);

        Task<EquipmentUpdateDTO> PutEquipmentAsync(int id, List<string> fieldsToUpdate, EquipmentUpdateDTO updates, Equipment entityToUpdate);

        Task<IEnumerable<Equipment>> UpdateStatusEquipmentAsync(List<int> EquipmentIds, bool equipmentStatus);

        Task<Equipment> DeleteEquipmentAsync(int id);

        Task<List<Equipment>> GetEquipmentsByIdAsync(List<int> equipmentIds);
    }
}
