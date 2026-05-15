using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HeadlessCms.Interfaces;
using HeadlessCms.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace HeadlessCms.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        private readonly SymmetricSecurityKey _key;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, IDistributedCache cache, ILogger<TokenService> logger)
        {
            _config = config;
            _userManager = userManager;
            _cache = cache;
            var signingKeyString = _config["JWT:SigningKey"]; // S6781: Key is loaded from secure source (User Secrets/Environment Variables, not from appsettings.json)
            if (string.IsNullOrWhiteSpace(signingKeyString))
            {
                throw new InvalidOperationException("JWT:SigningKey is not configured. Configure it via User Secrets (development) or environment variables (production).");
            }
            _key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKeyString)); // NOSONAR - S6781
            _logger = logger;
        }

        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            _logger.LogInformation("Creating JWT token for user {Email} with roles: {Roles}", user.Email, string.Join(", ", roles));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(1),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            var finalToken = tokenHandler.WriteToken(token);
            _logger.LogInformation("JWT token created successfully for user {Email}", user.Email);
            return finalToken;
        }

        public async Task<string> CreateRefreshToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var finalToken = tokenHandler.WriteToken(token);

            _logger.LogInformation("Refresh token created successfully for user {Email}", user.Email);
            return finalToken;
        }

        public async Task LoginWithGoogleAsync(ClaimsPrincipal claimsPrincipal, HttpContext context)
        {
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Google login failed: email claim is missing.");
                throw new InvalidOperationException("Google login failed: email claim is missing.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("No existing user found for Google login with email {Email}. Creating new user.", email);
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = claimsPrincipal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                    LastName = claimsPrincipal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user for Google login with email {Email}. Errors: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to create user for Google login: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign 'User' role to new user created for Google login with email {Email}. Errors: {Errors}", email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to assign 'User' role for Google login: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }

                _logger.LogInformation("New user created successfully for Google login with email {Email}", email);
                
            }

            var info = new UserLoginInfo("Google",
            claimsPrincipal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
             "Google");

            var loginResult = await _userManager.AddLoginAsync(user, info);
             
             if(!loginResult.Succeeded)
             {
                _logger.LogError("Failed to add Google login info for user with email {Email}. Errors: {Errors}", email, string.Join(", ", loginResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"Failed to add Google login info: {string.Join(", ", loginResult.Errors.Select(e => e.Description))}");
             }

            var jwtToken = await CreateToken(user);
            var refreshToken = await CreateRefreshToken(user);

            context.Response.Cookies.Append("ACCESS_TOKEN", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            context.Response.Cookies.Append("REFRESH_TOKEN", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            _logger.LogInformation("Google login tokens issued successfully for user {Email}", email);
        }

        public async Task Logout(string jti)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            };

            await _cache.SetStringAsync(jti, "revoked", options);
            _logger.LogInformation("User logged out successfully with JTI: {JTI}", jti);
        }
    }
}