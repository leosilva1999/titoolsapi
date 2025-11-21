using Microsoft.AspNetCore.Mvc;
using TiTools_backend.DTOs;
using TiTools_backend.Models;

namespace TiTools_backend.Services
{
    public interface IEquipmentService
    {
        Task<(IEnumerable<Equipment> List, int Count)> GetEquipmentsAsync(int limit, int offset, EquipmentFilterDTO filter);
        Task<IEnumerable<object>> GetEquipmentWithLoans(int id);
        Task<Equipment> PostEquipment(Equipment model);
        Task<EquipmentUpdateDTO> PutEquipment(int id, EquipmentUpdateDTO updates);
        Task<IEnumerable<Equipment>> UpdateStatusEquipment(List<int> EquipmentIds, bool equipmentStatus);

    }
}
