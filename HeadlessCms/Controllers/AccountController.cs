using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeadlessCms.Dtos.Account;
using HeadlessCms.Mappers;
using HeadlessCms.Models;
using HeadlessCms.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HeadlessCms.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace HeadlessCms.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]/")]

    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenservice;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenservice, IEmailService emailService, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _tokenservice = tokenservice;
            _emailService = emailService;
            _env = env;
        }

        private void WriteAuthTokenCookie(string name, string value, TimeSpan lifetime)
        {
            Response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction() ? true : Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(lifetime)
            });
        }

        [ApiVersion("1.0")]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(ModelState);
            // }

            if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.Firstname,
                LastName = registerDto.Lastname
            };

            var newUser = await _userManager.CreateAsync(user, registerDto.Password);
            if (!newUser.Succeeded)
            {
                return BadRequest(newUser.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
                 var verifyToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            BackgroundJob.Enqueue<IEmailService>(x =>
               x.SendVerifyUserEmail(
                   user.FirstName,
                   user.Email,
                     verifyToken));

            var responseDto = registerDto.ToRegisterResponseDto("User registered successfully, Check your mail");
            return Ok(responseDto);
        }

        [ApiVersion("1.0")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Email or username");
                }
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid Email or username");
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return Unauthorized("Please verify your email before logging in.");
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!passwordValid)
                {
                    return Unauthorized("Invalid password");
                }
                var token = await _tokenservice.CreateToken(user);
                var refreshToken = await _tokenservice.CreateRefreshToken(user);

                WriteAuthTokenCookie("ACCESS_TOKEN", token, TimeSpan.FromHours(1));
                WriteAuthTokenCookie("REFRESH_TOKEN", refreshToken, TimeSpan.FromDays(7));

                var responsedto = loginDto.ToLoginResponseDto(user, token);

                return Ok(responsedto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [ApiVersion("1.0")]
        [HttpGet("verify_email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Email and token are required.");
            }

            // ASP.NET Core sometimes decodes '+' as a space (' ') in query parameters.
            // Identity tokens often include '+' characters, so we map them back here.
            token = token.Replace(" ", "+");

            try
            {
                await _emailService.VerifyEmail(email, token);
                return Ok("Email verified successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Email verification failed: {ex.Message}");
            }
        }

        [ApiVersion("1.0")]
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var jti = User.GetJti();
            if (string.IsNullOrWhiteSpace(jti))
            {
                return BadRequest("Token jti is missing.");
            }
            await _tokenservice.Logout(jti);
            return Ok("Logged out successfully");
        }

        [ApiVersion("1.0")]
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User deleted successfully");
        }

        [ApiVersion("1.0")]
        [HttpGet("login/google")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string returnUrl, LinkGenerator linkGenerator,
            SignInManager<AppUser> signInManager)
        {
            var redirectUrl = linkGenerator.GetUriByAction(HttpContext, action: nameof(GoogleLoginCallback), controller: "Account", values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [ApiVersion("1.0")]
        [HttpGet("login/google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginCallback([FromQuery] string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            await _tokenservice.LoginWithGoogleAsync(result.Principal, HttpContext);

            // Prevent open-redirect vulnerabilities: only redirect to local URLs supplied by the user.
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback to a safe default when returnUrl is missing or not local.
            return Redirect("/");
        }
    }
}