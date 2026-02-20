using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TiTools_backend.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public LoanService(ILoanRepository loanRepository, IEquipmentRepository equipmentRepository)
        {
            _loanRepository = loanRepository;
            _equipmentRepository = equipmentRepository;
        }

        public async Task<(IEnumerable<Loan> loanList, int loanCount)> GetLoansAsync(int limit, int offset, LoanFilterDTO filter)
        {
            try
            {
                return await _loanRepository.GetLoansAsync(limit, offset, filter);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Loan> GetLoanAsync(int id)
        {
            try
            {
                return await _loanRepository.GetLoanAsync(id);
            }catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<LoanUpdateDTO> PutLoan(int id, LoanUpdateDTO updates)
        {
            try
            {
                var entityToUpdate = await _loanRepository.GetLoanAsync(id);

                if (entityToUpdate == null)
                    throw new KeyNotFoundException("Loan not found!");

                if (updates.ReturnTime < updates.RequestTime)
                    throw new InvalidOperationException("A data de recebimento é menor que a data de empréstimo");


                var fieldsToUpdate = new List<string>();

                if (updates.LoanStatus != null) fieldsToUpdate.Add("LoanStatus");
                if (updates.ApplicantName != null) fieldsToUpdate.Add("ApplicantName");
                if (updates.AuthorizedBy != null) fieldsToUpdate.Add("AuthorizedBy");
                if (updates.RequestTime != null) fieldsToUpdate.Add("RequestTime");
                if (updates.ReturnTime != null) fieldsToUpdate.Add("ReturnTime");

                if (updates.EquipmentIds != null && updates.EquipmentIds.Any())
                {
                    var existingEquipments = entityToUpdate.Equipments
                        .ToList();

                    var newEquipments = await _equipmentRepository.GetEquipmentsByIdAsync(updates.EquipmentIds);

                    foreach (var equipment in existingEquipments)
                    {
                        if (!newEquipments.Any(e => e.EquipmentId == equipment.EquipmentId))
                        {
                            entityToUpdate.Equipments.Remove(equipment);
                        }
                    }

                    foreach (var equipment in newEquipments)
                    {
                        if (!existingEquipments.Any(e => e.EquipmentId == equipment.EquipmentId))
                        {
                            entityToUpdate.Equipments.Add(equipment);
                        }
                    }
                }

                return await _loanRepository.PutLoan(id, fieldsToUpdate, updates, entityToUpdate);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Loan> PostLoan(LoanRequestDTO loanDTO)
        {
            try
            {
                var loan = new Loan
                {
                    ApplicantName = loanDTO.ApplicantName,
                    AuthorizedBy = loanDTO.AuthorizedBy,
                    RequestTime = loanDTO.RequestTime,
                    ReturnTime = loanDTO.ReturnTime,
                    LoanStatus = true,
                    Equipments = await _equipmentRepository.GetEquipmentsByIdAsync(loanDTO.EquipmentIds),
                };

                if (loanDTO.ReturnTime < loanDTO.RequestTime)
                    throw new InvalidOperationException("A data de recebimento é menor que a data de empréstimo");

                return await _loanRepository.PostLoanAsync(loanDTO);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<Loan> DeleteLoanAsync(int id)
        {
            try
            {
                return await _loanRepository.DeleteLoanAsync(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Loan> DeleteEquipmentFromLoan(int id)
        {
            try
            {
                return await _loanRepository.DeleteEquipmentFromLoan(id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
