using System.Security.Claims;
using System.Threading.Tasks;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.Controllers
{
    [Authorize]
    [Route("theme")] 
    public sealed class ThemeController : Controller
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [HttpPost("set")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Set([FromQuery] string theme)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            await _themeService.SetUserThemeAsync(userId, theme);
            _themeService.SetThemeCookie(Response, theme);
            return Ok(new { theme });
        }
    }
}


