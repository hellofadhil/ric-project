using System.Text.RegularExpressions;
using Core.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnePro.Front.Helpers;
using OnePro.Front.Mappers;
using OnePro.Front.Middleware;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers.Ric
{
    public class RicUserController : RicControllerBase
    {
        private const string ViewUserIndex = "~/Views/Ric/User/Index.cshtml";
        private const string ViewUserCreate = "~/Views/Ric/User/Create.cshtml";
        private const string ViewUserEdit = "~/Views/Ric/User/Edit.cshtml";
        private const string ViewUserUpdate = "~/Views/Ric/User/Update.cshtml";

        private const string ViewUserApprovalIndex = "~/Views/Ric/Approval/Index.cshtml";
        private const string ViewUserApprovalDetail = "~/Views/Ric/Approval/Detail.cshtml";

        public RicUserController(
            IRicService ricService,
            ILogger<RicUserController> logger,
            IWebHostEnvironment env
        )
            : base(ricService, logger, env) { }

        [RoleRequired(Role.User_Member, Role.User_Pic, Role.User_Manager, Role.User_VP)]
        [HttpGet("Ric/User")]
        public async Task<IActionResult> UserIndex()
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var rics = await RicService.GetMyRicsAsync(token);
            return View(ViewUserIndex, rics);
        }

        [HttpGet("Ric/User/Create")]
        public IActionResult Create()
        {
            if (!TryGetToken(out _))
                return RedirectToLogin();
            return View(ViewUserCreate, new RicCreateViewModel());
        }

        [HttpPost("Ric/User/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserCreate, model);

            try
            {
                var dto = await RicMapper.MapToCreateRequestAsync(model, action, SaveFilesAsync);
                await RicService.CreateRicAsync(dto, token);

                TempData["SuccessMessage"] = "RIC berhasil dibuat!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating RIC");
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat membuat RIC.");
                return View(ViewUserCreate, model);
            }
        }

        [HttpGet("Ric/User/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            if ((StatusRic)ric.Status != StatusRic.Draft)
                return RejectByStatus(
                    "RIC hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            var vm = RicMapper.MapToEditViewModel(ric);
            ModelState.Clear();
            return View(ViewUserEdit, vm);
        }

        [HttpPost("Ric/User/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserEdit, model);

            var existing = await RicService.GetRicByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if ((StatusRic)existing.Status != StatusRic.Draft)
                return RejectByStatus(
                    "RIC hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            try
            {
                var dto = await RicMapper.MapToUpdateRequestAsync(
                    id,
                    model,
                    action,
                    existing,
                    SaveFilesAsync
                );
                await RicService.UpdateRicAsync(id, dto, token);

                TempData["SuccessMessage"] = "RIC berhasil diperbarui!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating RIC {Id}", id);
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat update RIC.");
                return View(ViewUserEdit, model);
            }
        }

        [HttpGet("Ric/User/Update/{id:guid}")]
        public async Task<IActionResult> Update(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            if ((StatusRic)ric.Status != StatusRic.Return_BR_To_User)
                return RejectByStatus(
                    "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
                    nameof(UserIndex)
                );

            var vm = RicMapper.MapToEditViewModel(ric);
            ModelState.Clear();
            return View(ViewUserUpdate, vm);
        }

        [HttpPost("Ric/User/Update/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserUpdate, model);

            var existing = await RicService.GetRicByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if ((StatusRic)existing.Status != StatusRic.Return_BR_To_User)
                return RejectByStatus(
                    "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
                    nameof(UserIndex)
                );

            try
            {
                var dto = await RicMapper.MapToResubmitRequestAsync(
                    model,
                    action,
                    existing,
                    SaveFilesAsync
                );
                await RicService.ResubmitRicAsync(id, dto, token);

                TempData["SuccessMessage"] = "RIC berhasil di-resubmit!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error resubmitting RIC {Id}", id);
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat resubmit RIC.");
                return View(ViewUserUpdate, model);
            }
        }

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpGet("Ric/User/Approval")]
        public async Task<IActionResult> ApprovalIndex()
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            // reuse list my rics, nanti difilter di view (atau bisa filter di controller)
            var rics = await RicService.GetMyRicsAsync(token);

            // tampilkan yang lagi approval (pipeline user)
            var approvalRics = rics.Where(x =>
                    x.Status == StatusRic.Approval_Manager_User.ToString()
                    || x.Status == StatusRic.Approval_VP_User.ToString()
                    || x.Status == StatusRic.Approval_Manager_BR.ToString()
                    || x.Status == StatusRic.Approval_Manager_SARM.ToString()
                    || x.Status == StatusRic.Approval_VP_SARM.ToString()
                    || x.Status == StatusRic.Approval_Manager_ECS.ToString()
                    || x.Status == StatusRic.Approval_VP_ECS.ToString()
                )
                .ToList();

            return View(ViewUserApprovalIndex, approvalRics);
        }

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpGet("Ric/User/Approval/{id:guid}")]
        public async Task<IActionResult> Approval(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            return View(ViewUserApprovalDetail, ric);
        }

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpPost("Ric/User/Approval/{id:guid}/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAction(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ok = await RicService.ApproveAsync(id, token);
            if (!ok)
            {
                TempData["ErrorMessage"] =
                    "Gagal approve RIC. Cek status/role atau pending approval belum ada.";
                return RedirectToAction(nameof(Approval), new { id });
            }

            TempData["SuccessMessage"] = "RIC berhasil di-approve ✅";
            return RedirectToAction(nameof(Approval), new { id });
        }
    }
}
