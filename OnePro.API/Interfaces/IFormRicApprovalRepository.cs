using Core.Models.Entities;
using Core.ViewModels;

namespace OnePro.API.Interfaces
{
    public interface IFormRicApprovalRepository
    {
        Task<List<FormRicApprovalResponse>> GetAllAsync();
        Task<FormRicApprovalResponse?> GetByIdAsync(Guid id);

        Task<bool> CreateAsync(FormRicApproval model);
        Task<bool> UpdateAsync(FormRicApproval model);
        Task<bool> DeleteAsync(Guid id);
    }
}
