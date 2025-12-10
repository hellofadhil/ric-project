using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class RicRepository : IRicRepository
    {
        private readonly OneProDbContext _context;

        public RicRepository(OneProDbContext context)
        {
            _context = context;
        }

        public async Task<List<RicListItemResponse>> GetAllByGroupAsync(Guid groupId)
        {
            var query =
                from r in _context.FormRics.AsNoTracking()
                join u in _context.Users.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                where r.IdGroupUser == groupId
                select new RicListItemResponse
                {
                    Id = r.Id,
                    Judul = r.Judul,
                    Permasalahan = r.Permasalahan,
                    UserName = user != null ? user.Name : null,
                    Status = r.Status.ToString(),
                    UpdatedAt = r.UpdatedAt,
                };

            return await query.ToListAsync();
        }

        public async Task<FormRic?> GetByIdAsync(Guid id)
        {
            return await _context.FormRics!.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CreateAsync(FormRic model)
        {
            await _context.FormRics!.AddAsync(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(FormRic model)
        {
            _context.FormRics!.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.FormRics!.FindAsync(id);
            if (entity is null)
                return false;

            _context.FormRics.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
