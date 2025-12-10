// using Core.Models;
// using Core.Models.Entities;
// using Core.RequestModels;
// using Core.ViewModels;
// using Microsoft.Data.SqlClient;
// using Microsoft.EntityFrameworkCore;
// using OnePro.API.Interfaces;

// namespace OnePro.API.Repositories
// {
//     public class SettingRepository(OneProDbContext context) : ISettingRepository
//     {
//         private readonly OneProDbContext _context = context;

//         public async Task<List<Setting>> GetAsync()
//         {
//             var result = new List<Setting>();

//             try
//             {
//                 var data = await _context.Settings!.FromSqlRaw("EXEC SP_Portal_Settings_Get").ToListAsync();

//                 if (data.Count > 0)
//                 {
//                     result = data;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 var message = ex.Message;
//             }

//             return result;
//         }

//         public async Task<Setting> GetByIdAsync(Guid Id)
//         {
//             var result = new Setting();

//             try
//             {
//                 SqlParameter pId = new("@Id", Id);
//                 var data = await _context.Settings!.FromSqlRaw("EXEC SP_Portal_Settings_GetById @Id", pId).ToListAsync();

//                 if (data.Count > 0)
//                 {
//                     result = data.First();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 var message = ex.Message;
//             }

//             return result;
//         }

//         public async Task<bool> PostAsync(SettingRequest Model)
//         {
//             var result = false;
//             try
//             {
//                 SqlParameter pType = new("@Type", Model.Type);
//                 SqlParameter pId = new("@Id", Model.Id == default(Guid) ? Guid.NewGuid() : Model.Id);
//                 SqlParameter pConfig = new("@Config", Model.Config);
//                 SqlParameter pName = new("@Name", Model.Name);
//                 SqlParameter pValue = new("@Value", Model.Value);
//                 SqlParameter pCreatedBy = new("@CreatedBy", Model.CreatedBy);
//                 var data = await _context.Database.ExecuteSqlRawAsync(
//                     "EXEC SP_Portal_Settings_Upsert @Type,@Id,@Config,@Name,@Value,@CreatedBy"
//                     , pType
//                     , pId
//                     , pName
//                     , pConfig
//                     , pValue
//                     , pCreatedBy
//                     );

//                 if (data.Equals(-1))
//                 {
//                     result = true;
//                 }

//             }
//             catch (Exception ex)
//             {
//                 var message = ex.Message;
//             }

//             return result;
//         }

//     }
// }