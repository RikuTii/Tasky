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
using System.Security.Cryptography;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Models;

namespace Tasky.Controllers
{
    public class AccessToken
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
    }
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

        [HttpGet("Hello")]
        public IResult Hello()
        {
            return Results.Ok("hello");
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [AllowAnonymous]
        [HttpPost("CreateToken")]
        public IResult CreateToken([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var username = data["username"]?.ToString();
            var password = data["password"]?.ToString();
            Console.WriteLine("creating token");

            if (username == "joydip" && password == "joydip123")
            {
                var user = _context.Users.Where(e => e.Id == "8fed0172-93ca-4566-9f73-cba1e6988e48").Include(e => e.Account).First();
                if (user != null)
                {
                    /*var existingToken = _context.UserTokens.Where(e  => e.UserId == user.Id).First();

                    if(existingToken != null)
                    {
                        return Results.Ok(existingToken);
                    }*/
                    var account = user.Account;
                    account.RefreshToken = GenerateRefreshToken();
                    _context.Update(account);
                    _context.SaveChanges();

                }

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

                var result = new AccessToken
                {
                    access_token = stringToken,
                    refresh_token = user?.Account.RefreshToken
                };

                return Results.Ok<AccessToken>(result);
            }

            return Results.Unauthorized();
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz123456")),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }


        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public IResult RefreshToken([FromBody] JsonValue json)
        {
            JObject data = JObject.Parse(json.ToString());
            var accessToken = data["access_token"]?.ToString();
            var refreshToken = data["refresh_token"]?.ToString();
      
            var principal = GetPrincipalFromExpiredToken(accessToken);

            var sub = principal.Claims.Where(e => e.Type == ClaimTypes.NameIdentifier).First();
            if (sub != null)
            {
                var value = sub.Value.ToString();
                var user = _context.Users.Where(e => e.Id == value).Include(e => e.Account).First();
                if (user != null)
                {

                    var account = user.Account;
                    var refresh = account.RefreshToken;
                    if (refresh == refreshToken)
                    {
                        account.RefreshToken = GenerateRefreshToken();

                        _context.Update(account);
                        _context.SaveChanges();
                        Console.WriteLine("refreshed");

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
                        new Claim(JwtRegisteredClaimNames.Email, user.Id),
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

                        var result = new AccessToken
                        {
                            access_token = stringToken,
                            refresh_token = user?.Account.RefreshToken
                        };

                        return Results.Ok<AccessToken>(result);

                    }
                }

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
