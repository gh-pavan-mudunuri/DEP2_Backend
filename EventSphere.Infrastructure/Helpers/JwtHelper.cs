using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using EventSphere.Domain.Entities;

namespace backend.Helpers
{
    public class JwtHelper
    {
        private readonly string _jwtKey;
        public JwtHelper(string jwtKey)
        {
            // Ensure key is at least 128 bits (16+ chars)
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
            {
                _jwtKey = "supersecretkey123"; // 16 chars, fallback
            }
            else
            {
                _jwtKey = jwtKey;
            }
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
