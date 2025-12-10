using System.IO;
using System.Linq;
using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using OnePro.Front.Middleware;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers
{
    [AuthRequired]
    // [RoleRequired(1, 4)]
    public class RicController : Controller
    {
        private readonly IRicService _ricService;
        private readonly ILogger<RicController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RicController(
            IRicService ricService,
            ILogger<RicController> logger,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _ricService = ricService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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
                var dto = await MapToCreateRequestAsync(model, action);
                await _ricService.CreateRicAsync(dto, token);

                TempData["SuccessMessage"] = "RIC berhasil dibuat!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating RIC");
                ModelState.AddModelError("", "Terjadi kesalahan saat membuat RIC.");
                return View(model);
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

            _logger.LogInformation("RIC hasil after: {Val}", ric.HasilSetelahPerbaikan);


            var vm = MapToEditViewModel(ric);

            // Ini penting: buang ModelState lama biar nilai dari VM kepake di view
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
                var dto = await MapToUpdateRequestAsync(id, model, action, existing);
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

        private async Task<FormRicCreateRequest> MapToCreateRequestAsync(
            RicCreateViewModel model,
            string action
        )
        {
            var asIsUrl = await SaveFileAndReturnUrlAsync(model.AsIsRasciFile);
            var toBeUrl = await SaveFileAndReturnUrlAsync(model.ToBeProcessFile);
            var expectedUrl = await SaveFileAndReturnUrlAsync(model.ExpectedCompletionFile);

            var status = action == "submit" ? StatusRic.Submitted_To_BR : StatusRic.Draft;

            return new FormRicCreateRequest
            {
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,
                AsIsProcessRasciFile = CreateFileList(asIsUrl),
                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,
                AlternatifSolusi = model.Alternatifs,
                ToBeProcessBusinessRasciKkiFile = CreateFileList(toBeUrl),
                PotensiValueCreation = model.PotentialValue,
                ExcpectedCompletionTargetFile = CreateFileList(expectedUrl),
                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        private RicCreateViewModel MapToEditViewModel(RicDetailResponse ric)
        {
            return new RicCreateViewModel
            {
                Id = ric.Id,
                JudulPermintaan = ric.Judul,
                Hashtags = ric.Hastag ?? new List<string>(),
                Permasalahan = ric.Permasalahan ?? string.Empty,
                DampakMasalah = ric.DampakMasalah ?? string.Empty,
                FaktorPenyebab = ric.FaktorPenyebabMasalah ?? string.Empty,
                SolusiSaatIni = ric.SolusiSaatIni ?? string.Empty,
                Alternatifs = ric.AlternatifSolusi ?? new List<string>(),
                PotentialValue = ric.PotensiValueCreation,

                // Ini yang ngisi textbox Hasil Setelah Perbaikan
                HasilSetelahPerbaikan = ric.HasilSetelahPerbaikan ?? string.Empty,

                ExistingAsIsFileUrls = ric.AsIsProcessRasciFile,
                ExistingToBeFileUrls = ric.ToBeProcessBusinessRasciKkiFile,
                ExistingExpectedCompletionFileUrls = ric.ExcpectedCompletionTargetFile,
            };
        }

        private async Task<FormRicUpdateRequest> MapToUpdateRequestAsync(
            Guid id,
            RicCreateViewModel model,
            string action,
            RicDetailResponse existing
        )
        {
            var asIsUrl = await SaveFileAndReturnUrlAsync(model.AsIsRasciFile);
            var toBeUrl = await SaveFileAndReturnUrlAsync(model.ToBeProcessFile);
            var expectedUrl = await SaveFileAndReturnUrlAsync(model.ExpectedCompletionFile);

            var status = action == "submit" ? StatusRic.Submitted_To_BR : StatusRic.Draft;

            return new FormRicUpdateRequest
            {
                Id = id,
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,
                AsIsProcessRasciFile =
                    asIsUrl != null ? CreateFileList(asIsUrl) : existing.AsIsProcessRasciFile,
                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,
                AlternatifSolusi = model.Alternatifs,
                ToBeProcessBusinessRasciKkiFile =
                    toBeUrl != null
                        ? CreateFileList(toBeUrl)
                        : existing.ToBeProcessBusinessRasciKkiFile,
                PotensiValueCreation = model.PotentialValue,
                ExcpectedCompletionTargetFile =
                    expectedUrl != null
                        ? CreateFileList(expectedUrl)
                        : existing.ExcpectedCompletionTargetFile,
                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        private List<string>? CreateFileList(string? url)
        {
            return url != null ? new List<string> { url } : null;
        }

        private async Task<string?> SaveFileAndReturnUrlAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "ric"
            );
            Directory.CreateDirectory(uploadFolder);

            var fileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved: {FileName}", fileName);
            return $"/uploads/ric/{fileName}";
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(originalFileName);
            return $"{timestamp}_{uniqueId}{extension}";
        }

        private void LogModelStateErrors()
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    Key = x.Key,
                    Errors = x.Value!.Errors.Select(e => e.ErrorMessage).ToList(),
                });

            _logger.LogWarning("ModelState validation failed: {@Errors}", errors);
        }

        #endregion
    }
}
