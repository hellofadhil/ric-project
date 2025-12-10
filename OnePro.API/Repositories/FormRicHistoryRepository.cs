using Core.Models;
using Core.Models.Entities;
using Core.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class FormRicHistoryRepository(OneProDbContext context) : IFormRicHistoryRepository
    {
        private readonly OneProDbContext _context = context;

        public async Task<List<FormRicHistoryResponse>> GetAllAsync()
        {
            var data = await _context.FormRicHistoryResponses!
                .FromSqlRaw("EXEC SP_FormRicHistories_Get")
                .ToListAsync();

            return data;
        }

        public async Task<FormRicHistoryResponse?> GetByIdAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var data = await _context.FormRicHistoryResponses!
                .FromSqlRaw("EXEC SP_FormRicHistories_GetById @Id", pId)
                .ToListAsync();

            return data.FirstOrDefault();
        }

        public async Task<bool> CreateAsync(FormRicHistory model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@IdFormRic", model.IdFormRic),
                new SqlParameter("@IdEditor", model.IdEditor),
                new SqlParameter("@Version", model.Version),
                new SqlParameter("@Snapshot", model.Snapshot),
                new SqlParameter("@EditedFields", (object?)model.EditedFields ?? string.Empty)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_FormRicHistories_Create @Id,@IdFormRic,@IdEditor,@Version,@Snapshot,@EditedFields",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_FormRicHistories_Delete @Id",
                pId
            );

            return result > 0;
        }
    }
}
