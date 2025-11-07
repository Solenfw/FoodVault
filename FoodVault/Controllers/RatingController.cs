using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingController> _logger;

        public RatingController(IRatingService ratingService, ILogger<RatingController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRatingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid rating data.";
                return RedirectToAction("Details", "Recipe", new { id = vm.RecipeId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            try
            {
                await _ratingService.AddRatingAsync(userId, vm.RecipeId, vm.Rating, vm.Comment);
                TempData["Success"] = "Rating added successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add rating. user={UserId} recipe={RecipeId}", userId, vm.RecipeId);
                TempData["Error"] = "Failed to add rating.";
            }
            return RedirectToAction("Details", "Recipe", new { id = vm.RecipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string ratingId, string recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            try
            {
                var rating = await _ratingService.GetUserRatingForRecipeAsync(userId, recipeId);
                if (rating == null || rating.Id != ratingId)
                {
                    TempData["Error"] = "Rating not found or unauthorized.";
                    return RedirectToAction("Details", "Recipe", new { id = recipeId });
                }

                await _ratingService.DeleteRatingAsync(ratingId);
                TempData["Success"] = "Rating deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete rating {RatingId}", ratingId);
                TempData["Error"] = "Failed to delete rating.";
            }
            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }
    }
}

