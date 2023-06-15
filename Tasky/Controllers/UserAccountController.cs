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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace Tasky.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserAccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserAccountController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public void Register()
        {

        }
        [AllowAnonymous]
        [HttpPost("CreateToken")]
        public IResult CreateToken([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var username = data["username"]?.ToString();
            var password = data["password"]?.ToString();

            if (username == "joydip" && password == "joydip123")
            {

               /* var user = _context.Users.Where(e => e.Id == "8fed0172-93ca-4566-9f73-cba1e6988e48").First();
                if(user != null) 
                { 
                    var existingToken = _context.UserTokens.Where(e  => e.UserId == user.Id).First();

                    if(existingToken != null)
                    {
                        return Results.Ok(existingToken);
                    }
                }*/

                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var strKey = _configuration["Jwt:Key"];

                var key = Base64UrlEncoder.DecodeBytes(strKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("Id", Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Sub, "8fed0172-93ca-4566-9f73-cba1e6988e48"),
                        new Claim(JwtRegisteredClaimNames.Email, username),
                        new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials
                     (new SymmetricSecurityKey(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz123456")),
                     SecurityAlgorithms.HmacSha256)
                };



                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);

                /*if(user != null)
                {
                    var newToken = new IdentityUserToken<string>
                    {
                        UserId = user.Id,
                        Value = stringToken,
                        Name = "mobile",
                        LoginProvider = "mobile"
                    };
                    _context.Add(newToken);
                    _context.SaveChanges();
                }*/



                return Results.Ok(stringToken);
            }

            return Results.Unauthorized();
        }


        [Authorize]
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
