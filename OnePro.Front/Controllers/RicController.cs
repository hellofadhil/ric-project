using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using OnePro.Front.Middleware;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;
using OnePro.Front.Helpers;
using OnePro.Front.Mappers;

namespace OnePro.Front.Controllers
{
    [AuthRequired]
    public class RicController : Controller
    {
        private readonly IRicService _ricService;
        private readonly ILogger<RicController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public RicController(
            IRicService ricService,
            ILogger<RicController> logger,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env
        )
        {
            _ricService = ricService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        #region User Views

        [HttpGet("ric/user")]
        public async Task<IActionResult> UserIndex()
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var rics = await _ricService.GetMyRicsAsync(token);
            return View("~/Views/Ric/User/Index.cshtml", rics);
        }

        [RoleRequired(1, 4)]
        [HttpGet("ric/user/create")]
        public IActionResult Create()
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            return View("~/Views/Ric/User/Create.cshtml", new RicCreateViewModel());
        }

        [HttpPost("ric/user/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RicCreateViewModel model, string action)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            try
            {
                var dto = await RicMapper.MapToCreateRequestAsync(
                    model,
                    action,
                    files => FileStorageHelper.SaveRicFilesAsync(files, _env.WebRootPath, _logger)
                );

                await _ricService.CreateRicAsync(dto, token);

                TempData["SuccessMessage"] = "RIC berhasil dibuat!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RIC");
                ModelState.AddModelError("", "Terjadi kesalahan saat membuat RIC.");
                return View("~/Views/Ric/User/Create.cshtml", model);
            }
        }

        [HttpGet("ric/user/edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var ric = await _ricService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            if (ric.Status != (int)StatusRic.Draft)
            {
                TempData["ErrorMessage"] = "RIC hanya bisa diedit kalau status masih Draft.";
                return RedirectToAction(nameof(UserIndex));
            }

            var vm = RicMapper.MapToEditViewModel(ric);
            ModelState.Clear();

            return View("~/Views/Ric/User/Edit.cshtml", vm);
        }

        [HttpPost("ric/user/edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RicCreateViewModel model, string action)
        {
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var existing = await _ricService.GetRicByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if (existing.Status != (int)StatusRic.Draft)
            {
                TempData["ErrorMessage"] = "RIC hanya bisa diedit kalau status masih Draft.";
                return RedirectToAction(nameof(UserIndex));
            }

            try
            {
                var dto = await RicMapper.MapToUpdateRequestAsync(
                    id,
                    model,
                    action,
                    existing,
                    files => FileStorageHelper.SaveRicFilesAsync(files, _env.WebRootPath, _logger)
                );

                await _ricService.UpdateRicAsync(id, dto, token);

                TempData["SuccessMessage"] = "RIC berhasil diperbarui!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RIC {Id}", id);
                ModelState.AddModelError("", "Terjadi kesalahan saat update RIC.");
                return View("~/Views/Ric/User/Edit.cshtml", model);
            }
        }

        #endregion

        #region Private Helpers

        private string? GetAuthToken()
        {
            return HttpContext.Session.GetString("JwtToken");
        }

        #endregion
    }
}
