using Microsoft.EntityFrameworkCore;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;

namespace TiTools_backend.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentService(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        public async Task<Equipment> GetEquipmentAsync(int id)
        {
            try
            {
                return await _equipmentRepository.GetEquipmentAsync(id);
            }
            catch (Exception ex)
            {
                throw;
            }
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
                var entityToUpdate = await _equipmentRepository.GetEquipmentAsync(id);

                if (entityToUpdate == null) throw new KeyNotFoundException("Equipment not found!");

                var fieldsToUpdate = new List<string>();

                if (updates.EquipmentName != null) fieldsToUpdate.Add("EquipmentName");
                if (updates.IpAddress != null) fieldsToUpdate.Add("IpAddress");
                if (updates.MacAddress != null) fieldsToUpdate.Add("MacAddress");
                if (updates.Type != null) fieldsToUpdate.Add("Type");
                if (updates.Manufacturer != null) fieldsToUpdate.Add("Manufacturer");
                if (updates.Model != null) fieldsToUpdate.Add("Model");
                if (updates.EquipmentLoanStatus != null) fieldsToUpdate.Add("EquipmentLoanStatus");

                return await _equipmentRepository.PutEquipmentAsync(id, fieldsToUpdate, updates, entityToUpdate);
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
