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

        public async Task<AuthResult> RegisterAsync(RegisterDto model)
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
            var tenantName = !string.IsNullOrWhiteSpace(model.DisplayName) ? model.DisplayName : $"{model.Email}'s Workspace";
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

        public async Task<AuthResult> LoginAsync(LoginDto model)
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

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            // Look up the user directly via the asp_net_user_tokens table — O(1) indexed query
            var tokenRecord = await _context.UserTokens
                .FirstOrDefaultAsync(t => t.LoginProvider == "DailyNotes"
                                       && t.Name == "RefreshToken"
                                       && t.Value == refreshToken);

            if (tokenRecord == null)
                throw new Exception("Invalid refresh token.");

            var foundUser = await _userManager.FindByIdAsync(tokenRecord.UserId);

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

        private async Task<AuthResult> GenerateAuthResponse(IdentityUser user, int tenantId, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddHours(1);

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

            var rawRefreshToken = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            await _userManager.SetAuthenticationTokenAsync(user, "DailyNotes", "RefreshToken", rawRefreshToken);

            return new AuthResult
            {
                Response = new AuthResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expiry,
                    TenantId = tenantId.ToString(),
                    Role = role
                },
                RefreshToken = rawRefreshToken
            };
        }
    }
}
