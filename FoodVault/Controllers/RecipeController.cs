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
        private readonly IRatingService _ratingService;
        private readonly IFavoriteService _favoriteService;

        public RecipeController(IRecipeService recipeService, ILogger<RecipeController> logger, IImageService imageService, IRatingService ratingService, IFavoriteService favoriteService)
        {
            _recipeService = recipeService;
            _logger = logger;
            _imageService = imageService;
            _ratingService = ratingService;
            _favoriteService = favoriteService;
        }

        private sealed class TagSelection
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Redirect("/Identity/Account/Login");

                var recipes = await _recipeService.GetUserRecipesAsync(userId);
                var vms = recipes.Select(r => new RecipeListItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    UpdatedAt = r.UpdatedAt,
                    UserId = r.UserId
                }).ToList();
                return View(vms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipe index");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách công thức.";
                return View(new List<RecipeListItemViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRecipeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecipeViewModel vm, IFormFile? Image, string? TagsJson)
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

                // Upload image if provided
                if (Image != null && Image.Length > 0)
                {
                    try
                    {
                        var imageUrl = await _imageService.UploadImageAsync(Image, "recipe-thumbs");
                        entity.ImageUrl = imageUrl;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to upload image for recipe, continuing without image");
                        // Continue without image if upload fails
                    }
                }

                var recipe = await _recipeService.CreateRecipeAsync(entity);

                // Handle tags if provided
                if (!string.IsNullOrWhiteSpace(TagsJson))
                {
                    try
                    {
                        var tagSelections = System.Text.Json.JsonSerializer.Deserialize<List<TagSelection>>(TagsJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TagSelection>();
                        foreach (var t in tagSelections)
                        {
                            if (!string.IsNullOrEmpty(t.Id))
                            {
                                await _recipeService.AddTagAsync(recipe.Id, t.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse/add tags for recipe {RecipeId}", recipe.Id);
                    }
                }

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
                CookTimeMinutes = recipe.CookTimeMinutes,
                ImageUrl = recipe.ImageUrl
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
                var url = await _imageService.UploadImageAsync(thumbnail, "recipe-thumbs");
                var existing = await _recipeService.GetRecipeByIdAsync(id);
                if (existing == null)
                {
                    TempData["Error"] = "Recipe not found.";
                    return RedirectToAction(nameof(Index));
                }
                existing.ImageUrl = url;
                await _recipeService.UpdateRecipeAsync(existing);
                TempData["Success"] = "Thumbnail uploaded.";
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

            // Check if current user has favorited this recipe
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isFavorited = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isFavorited = await _favoriteService.IsFavoriteAsync(userId, recipe.Id);
            }

            // Load ratings with User information
            var ratings = await _ratingService.GetRatingsForRecipeAsync(recipe.Id);
            var ratingVMs = ratings.Select(r => new RatingViewModel
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.UserName ?? r.UserId,
                RecipeId = r.RecipeId,
                Rating = r.Rating1,
                Comment = r.Comment,
                RatedAt = r.RatedAt
            }).ToList();

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
                IsFavorited = isFavorited,
                Ratings = ratingVMs,
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Print(string id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound();

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

            var vm = new RecipeDetailsViewModel
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Servings = recipe.Servings,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                AuthorName = recipe.User?.UserName ?? recipe.UserId,
                Ingredients = ingredientVMs,
                TotalCalories = totalCal,
                TotalProtein = totalProtein,
                TotalFat = totalFat,
                TotalCarbs = totalCarb,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Steps = recipe.Steps?.OrderBy(s => s.StepNumber)
                    .Select(s => new StepViewModel { StepNumber = s.StepNumber, Instruction = s.Instruction }).ToList() ?? new List<StepViewModel>()
            };

            return View("Print", vm);
        }
    }
}


