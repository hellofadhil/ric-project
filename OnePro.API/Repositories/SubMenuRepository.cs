// using Core.Models;
// using Core.Models.Entities;
// using Microsoft.EntityFrameworkCore;
// using OnePro.API.Interfaces;

// namespace OnePro.API.Repositories
// {
//     public class SubMenuRepository(OneProDbContext context) : ISubMenuRepository
//     {
//         private readonly OneProDbContext _context = context;

//         public async Task<List<SubMenu>> GetAsync()
//         {
//             var result = new List<SubMenu>();

//             // var data = await _context.SubMenus!.FromSqlRaw("EXEC SP_Portal_SubMenu_Get").ToListAsync();

//             // Sesuai kebutuhan
//             var data = await _context.SubMenus!.ToListAsync();

//             if (data.Count > 0)
//             {
//                 result = data;
//             }

//             return result;
//         }
//     }
// }
