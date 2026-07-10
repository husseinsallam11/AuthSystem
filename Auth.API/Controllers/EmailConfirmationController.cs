using Auth.Application.Configuration;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class EmailConfirmationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender<ApplicationUser> _emailSender;
        private readonly EmailSettings _emailSettings;

        public EmailConfirmationController (
            UserManager<ApplicationUser> userManager,
            IEmailSender<ApplicationUser> emailSender,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailSettings = emailSettings.Value;
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Missing userId or token." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.EmailConfirmed)
                return Ok(new { message = "Email already Confirmed." });

            token = System.Net.WebUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { message = "Invalid or expire token", errors = result.Errors });

            return Ok(new { message = "Email Confirmed." });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "Email is required" });

            var user = await _userManager.FindByEmailAsync(request.Email);

            if(user is not null && !user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink =
                    $"{_emailSettings.ClientBaseUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink);
            }

            return Ok(new { message = "new confirmation mail has been sent" });
        }
    }
}

public record ResendConfirmationRequest(string Email);