using Core.Models.Entities;
using Core.ViewModels;

namespace OnePro.API.Interfaces
{
    public interface IFormRicHistoryRepository
    {
        Task<List<FormRicHistoryResponse>> GetAllAsync();
        Task<FormRicHistoryResponse?> GetByIdAsync(Guid id);

        Task<bool> CreateAsync(FormRicHistory model);
        Task<bool> DeleteAsync(Guid id);
    }
}
