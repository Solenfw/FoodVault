using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly IUserPreferencesService _prefsService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, IImageService imageService, IUserPreferencesService prefsService, ILogger<ProfileController> logger)
        {
            _userService = userService;
            _imageService = imageService;
            _prefsService = prefsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var profile = await _userService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            var stats = await _userService.GetUserStatsAsync(userId);
            var vm = new ProfileViewModel
            {
                Id = profile.Id,
                Name = profile.Name,
                Email = profile.Email,
                DietaryPreferences = profile.DietaryPreferences,
                DietaryRestrictions = profile.DietaryRestrictions,
                AvatarUrl = profile.AvatarUrl,
                NumRecipes = stats.NumRecipes,
                NumFavorites = stats.NumFavorites,
                NumFridges = stats.NumFridges
            };
            ViewBag.Theme = await _prefsService.GetThemeAsync(userId);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var profile = await _userService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                Id = profile.Id,
                Name = profile.Name,
                Email = profile.Email,
                DietaryPreferences = profile.DietaryPreferences,
                DietaryRestrictions = profile.DietaryRestrictions
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var profile = await _userService.GetProfileAsync(vm.Id);
                if (profile == null) return NotFound();

                profile.Name = vm.Name;
                profile.Email = vm.Email;
                profile.DietaryPreferences = vm.DietaryPreferences;
                profile.DietaryRestrictions = vm.DietaryRestrictions;

                await _userService.UpdateProfileAsync(profile);
                TempData["Success"] = "Profile updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update profile for {UserId}", vm.Id);
                TempData["Error"] = "Failed to update profile.";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (avatar == null)
            {
                TempData["Error"] = "No file uploaded.";
                return RedirectToAction(nameof(Edit));
            }

            try
            {
                var url = await _imageService.UploadImageAsync(avatar, "avatars");
                var profile = await _userService.GetProfileAsync(userId);
                if (profile == null) return NotFound();
                profile.AvatarUrl = url;
                await _userService.UpdateProfileAsync(profile);
                TempData["Success"] = "Avatar updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update avatar for {UserId}", userId);
                TempData["Error"] = "Failed to update avatar.";
            }
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTheme(string theme, [FromServices] IThemeService themeService)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
        await themeService.SetUserThemeAsync(userId, theme);
        themeService.SetThemeCookie(Response, theme);
            TempData["Success"] = "Theme updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}










