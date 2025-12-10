using System.Security.Claims;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RicController : ControllerBase
{
    private readonly IRicRepository _repository;

    public RicController(IRicRepository repository)
    {
        _repository = repository;
    }

    // --------------------------------------------
    // Helpers
    // --------------------------------------------
    private Guid? GetGuidClaim(string key)
    {
        var value = User.FindFirstValue(key);
        return Guid.TryParse(value, out var result) ? result : null;
    }

    private IActionResult MissingClaim(string field) => BadRequest($"{field} missing in token.");

    // --------------------------------------------
    // GET ALL RIC BY CURRENT USER GROUP
    // --------------------------------------------
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroupRics()
    {
        var groupId = GetGuidClaim("groupId");
        if (groupId is null)
            return MissingClaim("GroupId");

        var rics = await _repository.GetAllByGroupAsync(groupId.Value);
        return Ok(rics);
    }

    // --------------------------------------------
    // GET BY ID
    // --------------------------------------------
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ric = await _repository.GetByIdAsync(id);
        return ric is null ? NotFound("RIC not found.") : Ok(ric);
    }

    // --------------------------------------------
    // CREATE
    // --------------------------------------------
    [HttpPost]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Create([FromBody] FormRicRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuidClaim("id");
        var groupId = GetGuidClaim("groupId");

        if (userId is null)
            return MissingClaim("UserId");

        if (groupId is null)
            return MissingClaim("GroupId");

        var entity = new FormRic
        {
            IdUser = userId.Value,
            IdGroupUser = groupId.Value,

            Judul = request.Judul,
            Hastag = request.Hastag,
            AsIsProcessRasciFile = request.AsIsProcessRasciFile,
            Permasalahan = request.Permasalahan,
            DampakMasalah = request.DampakMasalah,
            FaktorPenyebabMasalah = request.FaktorPenyebabMasalah,
            SolusiSaatIni = request.SolusiSaatIni,
            AlternatifSolusi = request.AlternatifSolusi,
            ToBeProcessBusinessRasciKkiFile = request.ToBeProcessBusinessRasciKkiFile,
            PotensiValueCreation = request.PotensiValueCreation,
            ExcpectedCompletionTargetFile = request.ExcpectedCompletionTargetFile,
            HasilSetelahPerbaikan = request.HasilSetelahPerbaikan,

            BrConfirm = false,
            SarmConfirm = false,
            EcsConfirm = false,
            Status = request.Status,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var created = await _repository.CreateAsync(entity);
        if (!created)
            return StatusCode(500, "Failed to create RIC.");

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // --------------------------------------------
    // UPDATE
    // --------------------------------------------
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FormRicRequest request)
    {
        // Validasi model (biar konsisten sama POST)
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuidClaim("id");
        var groupId = GetGuidClaim("groupId");

        if (userId is null)
            return MissingClaim("UserId");

        if (groupId is null)
            return MissingClaim("GroupId");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound("RIC not found.");

        // Optional: kalau mau pastikan yang edit masih di group yang sama
        if (existing.IdGroupUser != groupId.Value)
            return Forbid(); // atau return Unauthorized("Different group");

        // Update field yang boleh diubah dari request DTO
        existing.Judul = request.Judul;
        existing.Hastag = request.Hastag;
        existing.AsIsProcessRasciFile = request.AsIsProcessRasciFile;
        existing.Permasalahan = request.Permasalahan;
        existing.DampakMasalah = request.DampakMasalah;
        existing.FaktorPenyebabMasalah = request.FaktorPenyebabMasalah;
        existing.SolusiSaatIni = request.SolusiSaatIni;
        existing.AlternatifSolusi = request.AlternatifSolusi;
        existing.ToBeProcessBusinessRasciKkiFile = request.ToBeProcessBusinessRasciKkiFile;
        existing.PotensiValueCreation = request.PotensiValueCreation;
        existing.ExcpectedCompletionTargetFile = request.ExcpectedCompletionTargetFile;
        existing.HasilSetelahPerbaikan = request.HasilSetelahPerbaikan;

        // Kalau Status memang boleh diubah oleh role di atas
        existing.Status = request.Status;

        // Audit
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return updated ? NoContent() : StatusCode(500, "Failed to update RIC.");
    }

    // --------------------------------------------
    // DELETE
    // --------------------------------------------
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        return deleted ? NoContent() : NotFound("RIC not found.");
    }


    
}
