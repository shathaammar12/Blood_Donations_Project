using Blood_Donations_Project.Models;
using Blood_Donations_Project.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blood_Donations_Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly BloodDonationContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher;

        public AuthService(BloodDonationContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<LoginResult?> LoginAsync(LoginViewModel model)
        {
            var user = _context.Users
            .FirstOrDefault(u => u.Email == model.Email);

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
                return null;

            var verifyResult = _hasher.VerifyHashedPassword(
            user,
            user.Password,
            model.Password
            );

            if (verifyResult != PasswordVerificationResult.Success)
                return null;

            var role = _context.Roles
            .FirstOrDefault(r => r.RoleId == user.RoleId);

            if (role == null || string.IsNullOrWhiteSpace(role.RoleName))
                return null;

            if (string.IsNullOrWhiteSpace(user.Email))
                return null;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, role.RoleName)
            };

            var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: model.RememberMe
                    ? DateTime.UtcNow.AddDays(7)
                    : DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new LoginResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Role = role.RoleName
            };
        }
    }
}
