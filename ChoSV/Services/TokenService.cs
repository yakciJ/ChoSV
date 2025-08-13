using ChoSV.Data;
using ChoSV.Models.Entities;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ChoSV.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDBContext _dbContext;
        public TokenService(IConfiguration configuration, SymmetricSecurityKey key, UserManager<User> userManager, ApplicationDBContext dbContext)
        {
            _configuration = configuration;
            _key = key;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var role = await _userManager.GetRolesAsync(user);
            if (role.Any())
            {
                claims.Add(new Claim(ClaimTypes.Role, role.First()));
            }

            var cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = cred,
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken, string? deviceInfo = null, string? ipAddress = null)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                DeviceInfo = deviceInfo,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
            };
            _dbContext.RefreshTokens.Add(token);
            await _dbContext.SaveChangesAsync();
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,  // Changed from false
                ValidateIssuer = true,    // Changed from false
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateLifetime = false, // This is correct - we don't validate expiry
                ValidIssuer = _configuration["JWT:Issuer"],      // Added
                ValidAudience = _configuration["JWT:Audience"]   // Added
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        }
        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task RevokeAllUserRefreshTokensAsync(string userId)
        {
            var refreshTokens = await _dbContext.RefreshTokens.Where(u => u.UserId == userId).ToListAsync();
            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.IsRevoked = true;
            }
            if (refreshTokens.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
