using OnePro.Front.Models;

namespace OnePro.Front.Services.Interfaces
{
    public interface IRicService
    {
        Task<List<RicItemResponse>> GetMyRicsAsync(string token);
        Task<RicDetailResponse?> GetRicByIdAsync(Guid id, string token);

        Task CreateRicAsync(FormRicCreateRequest request, string token);
        Task UpdateRicAsync(Guid id, FormRicUpdateRequest request, string token);
    }
}
