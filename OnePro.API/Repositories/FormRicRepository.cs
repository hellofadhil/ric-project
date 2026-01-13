// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Core.Models;
// using Core.Models.Entities;
// using Core.Models.Enums;
// using Microsoft.EntityFrameworkCore;
// using OnePro.API.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
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
            var groupBR = Guid.Parse("20000000-0000-0000-0000-000000000002");
            var groupSARM = Guid.Parse("30000000-0000-0000-0000-000000000003");
            var groupECS = Guid.Parse("40000000-0000-0000-0000-000000000004");

            var query =
                from r in _context.FormRics.AsNoTracking()
                join u in _context.Users.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                select new { Ric = r, User = user };

            // CASE 1: Group spesial (BR / SARM / ECS) => mode reviewer
            if (groupId == groupBR)
            {
                query = query.Where(x =>
                    x.Ric.Status == StatusRic.Submitted_To_BR
                    || x.Ric.Status == StatusRic.Review_BR
                    || x.Ric.Status == StatusRic.Return_SARM_To_BR
                    || x.Ric.Status == StatusRic.Return_ECS_To_BR
                );
            }
            else if (groupId == groupSARM)
            {
                query = query.Where(x => x.Ric.Status == StatusRic.Review_SARM);
            }
            else if (groupId == groupECS)
            {
                query = query.Where(x => x.Ric.Status == StatusRic.Review_ECS);
            }
            // CASE 2: Group biasa => mode user divisi (track semua status)
            else
            {
                query = query.Where(x => x.Ric.IdGroupUser == groupId);
            }

            return await query
                .Select(x => new RicListItemResponse
                {
                    Id = x.Ric.Id,
                    Judul = x.Ric.Judul,
                    Permasalahan = x.Ric.Permasalahan,
                    UserName = x.User != null ? x.User.Name : null,
                    Status = x.Ric.Status.ToString(),
                    UpdatedAt = x.Ric.UpdatedAt,
                })
                .ToListAsync();
        }

        public async Task<FormRic?> GetByIdAsync(Guid id)
        {
            return await _context.FormRics!.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        // public async Task<FormRic?> GetDetailByIdAsync(Guid id)
        // {
        //     // return await _context.FormRics!.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        //     return await _context
        //         .FormRics!.AsNoTracking()
        //         .Include(r => r.Histories)
        //         .Include(r => r.Reviews)
        //         .FirstOrDefaultAsync(r => r.Id == id);
        // }

        public async Task<FormRicDetailResponse?> GetDetailByIdAsync(Guid id)
        {
            var query =
                from r in _context.FormRics.AsNoTracking()
                where r.Id == id
                select new FormRicDetailResponse
                {
                    Id = r.Id,
                    Judul = r.Judul,

                    Hastag = r.Hastag,
                    AsIsProcessRasciFile = r.AsIsProcessRasciFile,
                    AlternatifSolusi = r.AlternatifSolusi,
                    ToBeProcessBusinessRasciKkiFile = r.ToBeProcessBusinessRasciKkiFile,

                    Permasalahan = r.Permasalahan,
                    DampakMasalah = r.DampakMasalah,
                    FaktorPenyebabMasalah = r.FaktorPenyebabMasalah,
                    SolusiSaatIni = r.SolusiSaatIni,

                    PotensiValueCreation = r.PotensiValueCreation,
                    ExcpectedCompletionTargetFile = r.ExcpectedCompletionTargetFile,
                    HasilSetelahPerbaikan = r.HasilSetelahPerbaikan,

                    Status = r.Status,
                    UpdatedAt = r.UpdatedAt,

                    Reviews = (
                        from rv in _context.ReviewFormRics.AsNoTracking()
                        join u in _context.Users.AsNoTracking()
                            on rv.IdUser equals u.Id
                            into userJoined
                        from user in userJoined.DefaultIfEmpty()
                        where rv.IdFormRic == r.Id
                        orderby rv.CreatedAt
                        select new ReviewRicResponse
                        {
                            Id = rv.Id,
                            Catatan = rv.Catatan,
                            RoleReview = rv.RoleReview.ToString(),
                            UserName = user != null ? user.Name : null,
                            CreatedAt = rv.CreatedAt,
                        }
                    ).ToList(),

                    Histories = (
                        from h in _context.FormRicHistories.AsNoTracking()
                        join u in _context.Users.AsNoTracking()
                            on h.IdEditor equals u.Id
                            into editorJoined
                        from editor in editorJoined.DefaultIfEmpty()
                        where h.IdFormRic == r.Id
                        orderby h.Version descending
                        select new RicHistoryResponse
                        {
                            Version = h.Version,
                            Snapshot = h.SnapshotJson,
                            EditedFields = h.EditedFieldsJson,
                            EditorName = editor != null ? editor.Name : null,
                            CreatedAt = h.CreatedAt,
                        }
                    ).ToList(),
                };

            return await query.FirstOrDefaultAsync();
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

        // public async Task<bool> ResubmitAfterRejection(FormRic model, Guid editorId)
        // {
        //     // 1. ambil data lama dari DB (tracked)
        //     var existing = await _context
        //         .FormRics!.AsNoTracking()
        //         .FirstOrDefaultAsync(x => x.Id == model.Id);

        //     if (existing is null)
        //         return false;

        //     // 2. cari versi terakhir
        //     var lastVersion = await _context
        //         .FormRicHistories!.Where(h => h.IdFormRic == model.Id)
        //         .OrderByDescending(h => h.Version)
        //         .Select(h => h.Version)
        //         .FirstOrDefaultAsync();

        //     var newVersion = lastVersion + 1;

        //     // 3. serialize snapshot versi lama
        //     var snapshotJson = JsonSerializer.Serialize(existing);

        //     // optional: hitung field yang berubah
        //     var editedFields = GetEditedFields(existing, model); // return string? JSON/dll

        //     var history = new FormRicHistory
        //     {
        //         IdFormRic = existing.Id,
        //         IdEditor = editorId,
        //         Version = newVersion,
        //         Snapshot = snapshotJson,
        //         EditedFields = editedFields,
        //         CreatedAt = DateTime.UtcNow,
        //     };

        //     await _context.FormRicHistories!.AddAsync(history);

        //     // 4. update entity utama ke versi baru
        //     _context.FormRics!.Update(model);

        //     return await _context.SaveChangesAsync() > 0;
        // }

        public async Task AddHistoryAsync(FormRicHistory history)
        {
            await _context.FormRicHistories!.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task AddReviewAsync(ReviewFormRic review)
        {
            await _context.ReviewFormRics!.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.FormRics!.FindAsync(id);
            if (entity is null)
                return false;

            _context.FormRics.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MoveRicToNextStageAsync(FormRic ric, Guid actorId)
        {
            var oldData = await _context
                .FormRics!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ric.Id);

            if (oldData is null)
                return false;

            var EditedFieldsJson = GetEditedFields(oldData, ric);

            if (EditedFieldsJson != null)
            {
                var lastVersion =
                    await _context
                        .FormRicHistories!.Where(x => x.IdFormRic == ric.Id)
                        .MaxAsync(x => (int?)x.Version) ?? 0;

                var history = new FormRicHistory
                {
                    Id = Guid.NewGuid(),
                    IdFormRic = oldData.Id,
                    IdEditor = actorId,
                    Version = lastVersion + 1,
                    SnapshotJson = JsonSerializer.Serialize(oldData),
                    EditedFieldsJson = EditedFieldsJson,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.FormRicHistories.Add(history);
            }

            _context.FormRics.Update(ric);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResubmitAfterRejection(FormRic newData, Guid editorId)
        {
            var oldData = await _context
                .FormRics!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == newData.Id);

            if (oldData is null)
                return false;

            var lastVersion =
                await _context
                    .FormRicHistories!.Where(x => x.IdFormRic == newData.Id)
                    .MaxAsync(x => (int?)x.Version) ?? 0;

            var history = new FormRicHistory
            {
                Id = Guid.NewGuid(),
                IdFormRic = oldData.Id,
                IdEditor = editorId,
                Version = lastVersion + 1,
                SnapshotJson = JsonSerializer.Serialize(oldData),
                EditedFieldsJson = GetEditedFields(oldData, newData),
                CreatedAt = DateTime.UtcNow,
            };

            _context.FormRicHistories.Add(history);
            _context.FormRics.Update(newData);

            return await _context.SaveChangesAsync() > 0;
        }

        // ========== HELPER DI BAWAH SINI ==========

        private static string? GetEditedFields(FormRic oldVal, FormRic newVal)
        {
            var changes = new Dictionary<string, object?>();

            static bool ListEquals(List<string>? a, List<string>? b)
            {
                if (a == b)
                    return true;
                if (a is null || b is null)
                    return false;
                if (a.Count != b.Count)
                    return false;

                for (int i = 0; i < a.Count; i++)
                {
                    if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                        return false;
                }
                return true;
            }

            if (!string.Equals(oldVal.Judul, newVal.Judul, StringComparison.Ordinal))
                changes["judul"] = newVal.Judul;

            if (!ListEquals(oldVal.Hastag, newVal.Hastag))
                changes["hastag"] = newVal.Hastag;

            if (!ListEquals(oldVal.AsIsProcessRasciFile, newVal.AsIsProcessRasciFile))
                changes["asIsProcessRasciFile"] = newVal.AsIsProcessRasciFile;

            if (!string.Equals(oldVal.Permasalahan, newVal.Permasalahan, StringComparison.Ordinal))
                changes["permasalahan"] = newVal.Permasalahan;

            if (
                !string.Equals(oldVal.DampakMasalah, newVal.DampakMasalah, StringComparison.Ordinal)
            )
                changes["dampakMasalah"] = newVal.DampakMasalah;

            if (
                !string.Equals(
                    oldVal.FaktorPenyebabMasalah,
                    newVal.FaktorPenyebabMasalah,
                    StringComparison.Ordinal
                )
            )
                changes["faktorPenyebabMasalah"] = newVal.FaktorPenyebabMasalah;

            if (
                !string.Equals(oldVal.SolusiSaatIni, newVal.SolusiSaatIni, StringComparison.Ordinal)
            )
                changes["solusiSaatIni"] = newVal.SolusiSaatIni;

            if (!ListEquals(oldVal.AlternatifSolusi, newVal.AlternatifSolusi))
                changes["alternatifSolusi"] = newVal.AlternatifSolusi;

            if (
                !ListEquals(
                    oldVal.ToBeProcessBusinessRasciKkiFile,
                    newVal.ToBeProcessBusinessRasciKkiFile
                )
            )
                changes["toBeProcessBusinessRasciKkiFile"] = newVal.ToBeProcessBusinessRasciKkiFile;

            if (
                !string.Equals(
                    oldVal.PotensiValueCreation,
                    newVal.PotensiValueCreation,
                    StringComparison.Ordinal
                )
            )
                changes["potensiValueCreation"] = newVal.PotensiValueCreation;

            if (
                !ListEquals(
                    oldVal.ExcpectedCompletionTargetFile,
                    newVal.ExcpectedCompletionTargetFile
                )
            )
                changes["excpectedCompletionTargetFile"] = newVal.ExcpectedCompletionTargetFile;

            if (
                !string.Equals(
                    oldVal.HasilSetelahPerbaikan,
                    newVal.HasilSetelahPerbaikan,
                    StringComparison.Ordinal
                )
            )
                changes["hasilSetelahPerbaikan"] = newVal.HasilSetelahPerbaikan;

            if (changes.Count == 0)
                return null;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            return JsonSerializer.Serialize(changes, options);
        }

        public async Task<bool> EnsureApprovalsCreatedAsync(Guid ricId)
        {
            // idempotent: kalau sudah ada record approval untuk ric ini, skip
            var exists = await _context.FormRicApprovals!.AnyAsync(a => a.IdFormRic == ricId);

            if (exists)
                return true;

            var now = DateTime.UtcNow;

            var approvals = Enum.GetValues(typeof(RoleApproval))
                .Cast<RoleApproval>()
                .Select(r => new FormRicApproval
                {
                    Id = Guid.NewGuid(),
                    IdFormRic = ricId,
                    IdApprover = Guid.Empty, // belum di-assign
                    Role = r,
                    ApprovalStatus = ApprovalStatus.Pending, // default Pending
                    ApprovalDate = null,
                    CreatedAt = now,
                })
                .ToList();

            await _context.FormRicApprovals!.AddRangeAsync(approvals);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkApprovalApprovedAsync(
            Guid ricId,
            RoleApproval role,
            Guid approverId
        )
        {
            var pending = await _context.FormRicApprovals!.FirstOrDefaultAsync(a =>
                a.IdFormRic == ricId && a.Role == role && a.ApprovalStatus == ApprovalStatus.Pending
            );

            if (pending is null)
                return false;

            pending.IdApprover = approverId;
            pending.ApprovalStatus = ApprovalStatus.Approved;
            pending.ApprovalDate = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
