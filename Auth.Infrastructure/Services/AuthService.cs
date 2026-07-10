using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Auth.Application.Interfaces;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Auth.Application.Configuration;
using Microsoft.Extensions.Options;


namespace Auth.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender<ApplicationUser> _emailSender;
        private readonly EmailSettings _emailSettings;

        public AuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailSender<ApplicationUser> emailSender,
            IOptions<EmailSettings> emailSettings)
                {
                    _userManager = userManager;
                    _signInManager = signInManager;
                    _configuration = configuration;
                    _emailSender = emailSender;
                    _emailSettings = emailSettings.Value;
                }

        public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists is not null)
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Email already registered"
                    });

            if(dto.Password != dto.ConfirmPassword)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Passwords doesn't match."
                    });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var CreateResult = await _userManager.CreateAsync(user, dto.Password);
            if (!CreateResult.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = string.Join(", ", CreateResult.Errors.Select(x => x.Description))
                });
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var ConfirmationLink = $"{_emailSettings.ClientBaseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendConfirmationLinkAsync(user, user.Email! , ConfirmationLink);

            return CreateResult;

        }
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid Email or Password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);

            if (result.IsNotAllowed)
            {
                throw new InvalidOperationException("Please confirm your email before logging in.");
            }

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Invalid Email or Password");
            }
            var response = GenerateJwtToken(user);

            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            response.RefreshToken = refreshToken;

            return response;



        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto)
        {
           
            var principal = GetPrincipalFromExpiredToken(dto.token);
            if (principal == null)
            {
                return null;
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            if(user.RefreshToken != dto.refreshToken)
            {
                return null;
            }


            if(user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            var response = GenerateJwtToken(user);

            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            response.RefreshToken = newRefreshToken;

            return response;
        }
        private AuthResponseDto GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
                 new Claim(ClaimTypes.Email, user.Email!),
                 new Claim(ClaimTypes.Name, user.UserName!)
             };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                ExpiresAt = expires
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,

                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwttoken || !jwttoken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase ))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return false;
            }
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
