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
            try { 
                return await _equipmentRepository.GetEquipmentsAsync(limit, offset, filter);
            }catch(Exception ex)
            {
                throw;
            }
}

        public async Task<IEnumerable<object>> GetEquipmentWithLoansAsync(int id)
        {
            try
            {
                return await _equipmentRepository.GetEquipmentWithLoansAsync(id);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Equipment> PostEquipmentAsync(Equipment model)
        {
            try
            {
                return await _equipmentRepository.PostEquipmentAsync(model);
            }
            catch(Exception ex)
            {           
                throw;
            }
        }

        public async Task<EquipmentUpdateDTO> PutEquipmentAsync(int id, EquipmentUpdateDTO updates)
        {
            try
            {
                return await _equipmentRepository.PutEquipmentAsync(id, updates);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Equipment>> UpdateStatusEquipmentAsync(List<int> EquipmentIds, bool equipmentStatus)
        {
            try
            {
                return await _equipmentRepository.UpdateStatusEquipmentAsync(EquipmentIds, equipmentStatus);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Equipment> DeleteEquipmentAsync(int id)
        {
            try
            {
                return await _equipmentRepository.DeleteEquipmentAsync(id);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
