// using Core.Models.Entities;
// using Core.ViewModels;

// namespace OnePro.API.Interfaces
// {
//     public interface IRicRepository
//     {
//         Task<List<FormRicResponse>> GetAllAsync();
//         Task<FormRicResponse?> GetByIdAsync(Guid id);

//         Task<bool> CreateAsync(FormRic model);
//         Task<bool> UpdateAsync(FormRic model);
//         Task<bool> DeleteAsync(Guid id);
//     }
// }

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models.Entities;

namespace OnePro.API.Interfaces
{
    public interface IRicRepository
    {
        Task<List<RicListItemResponse>> GetAllByGroupAsync(Guid groupId);
        Task<FormRic?> GetByIdAsync(Guid id);

        Task<bool> CreateAsync(FormRic model);
        Task<bool> UpdateAsync(FormRic model);
        Task<bool> DeleteAsync(Guid id);
    }
}
