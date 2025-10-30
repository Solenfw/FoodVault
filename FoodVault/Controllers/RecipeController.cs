using FoodVault.Models.Entities;
using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipeController> _logger;

        private readonly IImageService _imageService;

        public RecipeController(IRecipeService recipeService, ILogger<RecipeController> logger, IImageService imageService)
        {
            _recipeService = recipeService;
            _logger = logger;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var recipes = await _recipeService.GetUserRecipesAsync(userId);
            var vms = recipes.Select(r => new RecipeListItemViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                UpdatedAt = r.UpdatedAt
            }).ToList();
            return View(vms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRecipeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecipeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                var entity = new Recipe
                {
                    UserId = userId,
                    Title = vm.Title,
                    Description = vm.Description,
                    Servings = vm.Servings,
                    PrepTimeMinutes = vm.PrepTimeMinutes,
                    CookTimeMinutes = vm.CookTimeMinutes
                };
                await _recipeService.CreateRecipeAsync(entity);
                TempData["Success"] = "Recipe created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create recipe");
                TempData["Error"] = "Failed to create recipe.";
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound();
            var vm = new EditRecipeViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                Servings = recipe.Servings,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRecipeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            try
            {
                var existing = await _recipeService.GetRecipeByIdAsync(vm.Id);
                if (existing == null) return NotFound();
                existing.Title = vm.Title;
                existing.Description = vm.Description;
                existing.Servings = vm.Servings;
                existing.PrepTimeMinutes = vm.PrepTimeMinutes;
                existing.CookTimeMinutes = vm.CookTimeMinutes;
                await _recipeService.UpdateRecipeAsync(existing);
                TempData["Success"] = "Recipe updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update recipe {RecipeId}", vm.Id);
                TempData["Error"] = "Failed to update recipe.";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadThumbnail(string id, IFormFile thumbnail)
        {
            if (thumbnail == null)
            {
                TempData["Error"] = "No file uploaded.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            try
            {
                // No Recipe.ImageUrl field exists; store path transiently for demo
                var url = await _imageService.UploadImageAsync(thumbnail, "recipe-thumbs");
                TempData["Success"] = "Thumbnail uploaded.";
                TempData["RecipeThumbUrl"] = url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload thumbnail for recipe {RecipeId}", id);
                TempData["Error"] = "Failed to upload thumbnail.";
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIngredient(AddRecipeIngredientViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Edit), new { id = vm.RecipeId });
            try
            {
                var entity = new RecipeIngredient { RecipeId = vm.RecipeId, IngredientId = vm.IngredientId, Quantity = vm.Quantity, Unit = vm.Unit };
                await _recipeService.AddIngredientAsync(entity);
                TempData["Success"] = "Ingredient added.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add ingredient to recipe {RecipeId}", vm.RecipeId);
                TempData["Error"] = "Failed to add ingredient.";
            }
            return RedirectToAction(nameof(Edit), new { id = vm.RecipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveIngredient(RemoveRecipeIngredientViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
            try
            {
                await _recipeService.RemoveIngredientAsync(vm.RecipeIngredientId);
                TempData["Success"] = "Ingredient removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove ingredient {RecipeIngredientId}", vm.RecipeIngredientId);
                TempData["Error"] = "Failed to remove ingredient.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTag(AddRecipeTagViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Edit), new { id = vm.RecipeId });
            try
            {
                await _recipeService.AddTagAsync(vm.RecipeId, vm.TagId);
                TempData["Success"] = "Tag added.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add tag to recipe {RecipeId}", vm.RecipeId);
                TempData["Error"] = "Failed to add tag.";
            }
            return RedirectToAction(nameof(Edit), new { id = vm.RecipeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTag(RemoveRecipeTagViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
            try
            {
                await _recipeService.RemoveTagAsync(vm.RecipeTagId);
                TempData["Success"] = "Tag removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove tag {RecipeTagId}", vm.RecipeTagId);
                TempData["Error"] = "Failed to remove tag.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _recipeService.DeleteRecipeAsync(id);
                TempData["Success"] = "Recipe deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete recipe {RecipeId}", id);
                TempData["Error"] = "Failed to delete recipe.";
            }
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound();

            // Chắc chắn phải có navigation: Ingredients/RecipeIngredients, Ratings, Favorites, User
            var ingredientVMs = recipe.RecipeIngredients
                .Select(ri => new RecipeIngredientDetailsViewModel {
                    IngredientName = ri.Ingredient?.Name ?? ri.IngredientId,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit,
                    Calories = ri.Quantity * (ri.Ingredient?.DefaultCalories ?? 0),
                    Protein = ri.Quantity * (ri.Ingredient?.DefaultProtein ?? 0),
                    Fat = ri.Quantity * (ri.Ingredient?.DefaultFat ?? 0),
                    Carbs = ri.Quantity * (ri.Ingredient?.DefaultCarbs ?? 0)
                }).ToList();

            double totalCal = ingredientVMs.Sum(i => i.Calories ?? 0);
            double totalProtein = ingredientVMs.Sum(i => i.Protein ?? 0);
            double totalFat = ingredientVMs.Sum(i => i.Fat ?? 0);
            double totalCarb = ingredientVMs.Sum(i => i.Carbs ?? 0);

            double avgRating = (recipe.Ratings != null && recipe.Ratings.Any()) ? recipe.Ratings.Average(r => r.Rating1) : 0;
            int favoriteCount = recipe.Favorites?.Count ?? 0;
            string authorName = recipe.User?.UserName ?? recipe.UserId;

            var vm = new RecipeDetailsViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Servings = recipe.Servings,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                AuthorName = authorName,
                Ingredients = ingredientVMs,
                TotalCalories = totalCal,
                TotalProtein = totalProtein,
                TotalFat = totalFat,
                TotalCarbs = totalCarb,
                AvgRating = avgRating,
                FavoriteCount = favoriteCount,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Steps = recipe.Steps?.OrderBy(s => s.StepNumber)
                    .Select(s => new StepViewModel {
                        StepNumber = s.StepNumber,
                        Instruction = s.Instruction
                    }).ToList() ?? new List<StepViewModel>()
            };

            return View(vm);
        }
    }
}


