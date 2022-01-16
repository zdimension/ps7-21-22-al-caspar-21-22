using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PS7Api.Models;
using TwoFactorAuthNet;
using TwoFactorAuthNetSkiaSharpQrProvider;

namespace PS7Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private static readonly TwoFactorAuth AuthService =
        new("PolyFrontier", qrcodeprovider: new SkiaSharpQrCodeProvider());

    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public AuthController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    /// <summary>
    /// To login as an user
    /// </summary>
    /// <param name="body">The login form</param>
    /// <response code="200">The Bearer token with its expiration</response>
    /// <response code="401">Unauthorized - one or more login fields are wrong</response>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginBody body)
    {
        var user = await _userManager.FindByEmailAsync(body.Email);

        if (user == null)
            return Unauthorized();

        if (user.TwoFactorEnabled)
        {
            if (body.TwoFactorCode == null)
                return Unauthorized(new
                {
                    Message =
                        "Two factor authentication is enabled for this account. Please provide an authentication code."
                });

            var result = AuthService.VerifyCode(user.TwoFactorSecret, body.TwoFactorCode);

            if (!result)
                return Unauthorized(new
                {
                    Message = "Invalid two factor authentication code."
                });
        }

        if (!await _userManager.CheckPasswordAsync(user, body.Password))
            return Unauthorized();

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="body">The user's connection information</param>
    /// <response code="500">User already exist with the same email, or the creation has failed</response>
    /// <response code="200">User created</response>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] LoginBody body)
    {
        var userExists = await _userManager.FindByEmailAsync(body.Email);
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Status = "Error", Message = "User already exists!" });

        var user = new User
        {
            Email = body.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = body.Email
        };
        var result = await _userManager.CreateAsync(user, body.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new
                {
                    Status = "Error", Message = "User creation failed.",
                    Errors = result.Errors.Select(err => err.Description)
                });

        return Ok(new { Status = "Success", Message = "User created successfully!" });
    }

    /// <summary>
    /// Returns the the two factor secret of an user in image
    /// </summary>
    /// <param name="id">The email of the user</param>
    /// <response code="404">The given email isn't associate with an user</response>
    /// <response code="200">The image</response>
    [HttpGet("qr")]
    public async Task<IActionResult> GetQrCode(string id)
    {
        var user = await _userManager.FindByEmailAsync(id);
        if (user == null)
            return NotFound();

        return Ok(AuthService.GetQrCodeImageAsDataUri("PolyFrontier", user.TwoFactorSecret));
    }

    public record LoginBody([EmailAddress] string Email, string Password, string? TwoFactorCode = null);
}