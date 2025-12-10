using Core.Models;
using Core.Models.Entities;
using Core.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class FormRicApprovalRepository(OneProDbContext context) : IFormRicApprovalRepository
    {
        private readonly OneProDbContext _context = context;

        public async Task<List<FormRicApprovalResponse>> GetAllAsync()
        {
            return await _context.FormRicApprovalResponses!
                .FromSqlRaw("EXEC SP_FormRicApprovals_Get")
                .ToListAsync();
        }

        public async Task<FormRicApprovalResponse?> GetByIdAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var data = await _context.FormRicApprovalResponses!
                .FromSqlRaw("EXEC SP_FormRicApprovals_GetById @Id", pId)
                .ToListAsync();

            return data.FirstOrDefault();
        }

        public async Task<bool> CreateAsync(FormRicApproval model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@IdFormRic", model.IdFormRic),
                new SqlParameter("@IdApprover", model.IdApprover),
                new SqlParameter("@Role", model.Role.ToString()),
                new SqlParameter("@ApprovalStatus", model.ApprovalStatus.ToString()),
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_FormRicApprovals_Create @Id,@IdFormRic,@IdApprover,@Role,@ApprovalStatus",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> UpdateAsync(FormRicApproval model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@ApprovalStatus", model.ApprovalStatus.ToString())
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_FormRicApprovals_Update @Id,@ApprovalStatus",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_FormRicApprovals_Delete @Id",
                pId
            );

            return result > 0;
        }
    }
}
