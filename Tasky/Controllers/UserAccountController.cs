using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using Tasky.Data;
using Tasky.Models;

namespace Tasky.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public void Register()
        {

        }

        [HttpGet("Account")]
        public async Task<IActionResult> Account()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await Console.Out.WriteLineAsync("get user");
            ApplicationUser user = _context.Users.Where(e => e.Id == userId).Include(e => e.Account).First();
            if (user != null)
            {
                var account = await _context.UserAccount.Where(e => e.UserID == user.Id).ToListAsync();
                using (var stream = new MemoryStream())
                {
                    await JsonSerializer.SerializeAsync(stream, account,
                    new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    });

                    stream.Position = 0;
                    using var reader = new StreamReader(stream);
                    return Ok(await reader.ReadToEndAsync());
                }
            }

            return Problem("No user account");
        }
    }
}
