using Core.Models.Entities;

namespace OnePro.API.Interfaces
{
    public interface IRicRepository
    {
        Task<List<RicListItemResponse>> GetAllByGroupAsync(Guid groupId);
        Task<FormRic?> GetByIdAsync(Guid id);
        Task<FormRicDetailResponse?> GetDetailByIdAsync(Guid id);

        Task<bool> CreateAsync(FormRic model);
        Task<bool> UpdateAsync(FormRic model);
        Task<bool> ResubmitAfterRejection(FormRic model, Guid editorId);
        Task<bool> MoveRicToNextStageAsync(FormRic model, Guid actorId);

        Task<bool> DeleteAsync(Guid id);

        Task AddHistoryAsync(FormRicHistory history);
        Task AddReviewAsync(ReviewFormRic review);
    }
}
