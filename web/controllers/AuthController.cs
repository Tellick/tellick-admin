using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace tellick_admin.Controllers {
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        [AllowAnonymous]
        [HttpPost("login", Name = "Login")]
        public IActionResult Login([FromBody] AuthRequest request) {
            if (request.Username == "development" && request.Password == "development") { //@todo make it so it works with asp.net identity
                var claims = new[] {
                    new Claim(ClaimTypes.Name, request.Username)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSigningKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: Settings.JwtIssuer,
                    audience: Settings.JwtAudience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(28),
                    signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            return BadRequest("Could not verify username and password");
        }
    }
}