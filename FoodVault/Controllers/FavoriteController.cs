using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IFavoriteService favoriteService, ILogger<FavoriteController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var recipes = await _favoriteService.GetUserFavoriteRecipesAsync(userId);
            var vms = recipes.Select(r => new FavoriteListItemViewModel { RecipeId = r.Id, Title = r.Title }).ToList();
            return View(vms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            try
            {
                await _favoriteService.AddFavoriteAsync(userId, recipeId);
                TempData["Success"] = "Added to favorites.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
                TempData["Error"] = "Failed to add favorite.";
            }
            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            try
            {
                await _favoriteService.RemoveFavoriteAsync(userId, recipeId);
                TempData["Success"] = "Removed from favorites.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
                TempData["Error"] = "Failed to remove favorite.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}











