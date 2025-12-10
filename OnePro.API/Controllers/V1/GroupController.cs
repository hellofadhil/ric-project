using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _repo;

        public GroupController(IGroupRepository repo)
        {
            _repo = repo;
        }

        // ============================================================
        // GET CURRENT USER GROUP + MEMBERS
        // ============================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyGroup()
        {
            var groupIdStr = User.FindFirstValue("groupId");

            if (string.IsNullOrEmpty(groupIdStr))
                return BadRequest("User does not belong to any group.");

            var groupId = Guid.Parse(groupIdStr);

            var result = await _repo.GetGroupWithMembersAsync(groupId);

            if (result == null)
                return NotFound("Group not found.");

            return Ok(result);
        }

        // [HttpPost("invite")]
        // public async Task<IActionResult> Invite([FromBody] InviteRequest req)
        // {
        //     // Cek apakah user sudah terdaftar di sistem
        //     var user = await _userRepo.GetByEmailAsync(req.Email);
        //     if (user == null)
        //         return BadRequest("User tidak ditemukan di sistem.");

        //     // Cek apakah user sudah ada di group
        //     var exist = await _groupRepo.IsMember(req.GroupId, user.Id);
        //     if (exist)
        //         return BadRequest("User sudah menjadi member.");

        //     // Tambahkan user ke group
        //     await _groupRepo.AddMember(req.GroupId, user.Id);

        //     return Ok(new { success = true, message = "User berhasil diundang ke group." });
        // }
    }
}
