using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DailyNotes.Core.DTOs.Auth;
using DailyNotes.Core.Entities;
using DailyNotes.Core.Interfaces;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DailyNotes.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DailyNotesDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<IdentityUser> userManager,
            DailyNotesDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            // 1. Create Identity User
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }

            // 2. Create Tenant
            var tenantName = !string.IsNullOrWhiteSpace(model.TenantName) ? model.TenantName : $"{model.Email}'s Workspace";
            var tenant = new Tenant
            {
                Name = tenantName
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 3. Link User to Tenant (Owner)
            var tenantUser = new TenantUser
            {
                TenantId = tenant.Id,
                UserId = user.Id,
                Role = "owner",
                Preferences = System.Text.Json.JsonDocument.Parse("{}") // Default empty JSON
            };

            _context.TenantUsers.Add(tenantUser);
            await _context.SaveChangesAsync();

            // 4. Generate Token
            return await GenerateAuthResponse(user, tenant.Id, "owner");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            // 1. Validate User
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid email or password.");

            // 2. Get Tenant (Default to first one for now)
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == user.Id);

            if (tenantUser == null)
                throw new Exception("User is not associated with any tenant.");

            // 3. Generate Token
            return await GenerateAuthResponse(user, tenantUser.TenantId, tenantUser.Role);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Find user by stored refresh token
            var users = _userManager.Users.ToList();
            IdentityUser? foundUser = null;

            foreach (var u in users)
            {
                var storedToken = await _userManager.GetAuthenticationTokenAsync(u, "DailyNotes", "RefreshToken");
                if (storedToken == refreshToken)
                {
                    foundUser = u;
                    break;
                }
            }

            if (foundUser == null)
                throw new Exception("Invalid refresh token.");

            // Get tenant
            var tenantUser = await _context.TenantUsers
                .FirstOrDefaultAsync(tu => tu.UserId == foundUser.Id);

            if (tenantUser == null)
                throw new Exception("User is not associated with any tenant.");

            // Generate new tokens
            return await GenerateAuthResponse(foundUser, tenantUser.TenantId, tenantUser.Role);
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(IdentityUser user, int tenantId, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(7);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("role", role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            // Generate and store refresh token
            var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            await _userManager.SetAuthenticationTokenAsync(user, "DailyNotes", "RefreshToken", refreshToken);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = expiry,
                TenantId = tenantId.ToString(),
                Role = role
            };
        }
    }
}
