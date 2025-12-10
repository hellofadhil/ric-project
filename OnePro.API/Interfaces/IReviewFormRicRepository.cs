using Core.Models.Entities;
using Core.ViewModels;

namespace OnePro.API.Interfaces
{
    public interface IReviewFormRicRepository
    {
        Task<List<ReviewFormRicResponse>> GetAllAsync();
        Task<ReviewFormRicResponse?> GetByIdAsync(Guid id);

        Task<bool> CreateAsync(ReviewFormRic model);
        Task<bool> UpdateAsync(ReviewFormRic model);
        Task<bool> DeleteAsync(Guid id);
    }
}
