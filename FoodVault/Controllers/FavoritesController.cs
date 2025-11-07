using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(IFavoriteService favoriteService, ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please login to favorite recipes.";
                return RedirectToAction("Details", "Recipe", new { id = recipeId });
            }

            try
            {
                var isFavorite = await _favoriteService.IsFavoriteAsync(userId, recipeId);
                if (isFavorite)
                {
                    await _favoriteService.RemoveFavoriteAsync(userId, recipeId);
                    TempData["Success"] = "Removed from favorites.";
                }
                else
                {
                    await _favoriteService.AddFavoriteAsync(userId, recipeId);
                    TempData["Success"] = "Added to favorites.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite. user={UserId} recipe={RecipeId}", userId, recipeId);
                TempData["Error"] = "Failed to update favorite.";
            }
            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }
    }
}

