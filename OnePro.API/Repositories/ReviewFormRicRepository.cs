using Core.Models;
using Core.Models.Entities;
using Core.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class ReviewFormRicRepository(OneProDbContext context) : IReviewFormRicRepository
    {
        private readonly OneProDbContext _context = context;

        public async Task<List<ReviewFormRicResponse>> GetAllAsync()
        {
            var data = await _context.ReviewFormRicResponses!
                .FromSqlRaw("EXEC SP_ReviewFormRics_Get")
                .ToListAsync();

            return data;
        }

        public async Task<ReviewFormRicResponse?> GetByIdAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var data = await _context.ReviewFormRicResponses!
                .FromSqlRaw("EXEC SP_ReviewFormRics_GetById @Id", pId)
                .ToListAsync();

            return data.FirstOrDefault();
        }

        public async Task<bool> CreateAsync(ReviewFormRic model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@IdFormRic", model.IdFormRic),
                new SqlParameter("@IdUser", model.IdUser),
                new SqlParameter("@Catatan", (object?)model.Catatan ?? string.Empty),
                new SqlParameter("@RoleReview", model.RoleReview.ToString())
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_ReviewFormRics_Create @Id,@IdFormRic,@IdUser,@Catatan,@RoleReview",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> UpdateAsync(ReviewFormRic model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@Catatan", (object?)model.Catatan ?? string.Empty)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_ReviewFormRics_Update @Id,@Catatan",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_ReviewFormRics_Delete @Id", pId
            );

            return result > 0;
        }
    }
}
