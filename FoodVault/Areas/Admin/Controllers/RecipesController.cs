using System;
using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly IFavoriteService _favoriteService;
        private readonly IRatingService _ratingService;
        private readonly FoodVault.Models.Data.FoodVaultDbContext _dbContext;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(
            IRecipeService recipeService,
            IFavoriteService favoriteService,
            IRatingService ratingService,
            FoodVault.Models.Data.FoodVaultDbContext dbContext,
            ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _favoriteService = favoriteService;
            _ratingService = ratingService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Recipes
                    .Include(r => r.User)
                    .Include(r => r.Favorites)
                    .Include(r => r.Ratings)
                    .Include(r => r.RecipeIngredients)
                    .Include(r => r.Steps)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(r => 
                        r.Title.ToLower().Contains(searchLower) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchLower)));
                }

                var totalRecipes = await query.CountAsync();
                var recipes = await query
                    .OrderByDescending(r => r.CreatedAt ?? DateTime.MinValue)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = recipes.Select(r => new RecipeApprovalListViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    AuthorName = r.User?.UserName ?? r.UserId,
                    AuthorId = r.UserId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    FavoriteCount = r.Favorites?.Count ?? 0,
                    AvgRating = r.Ratings != null && r.Ratings.Any() ? r.Ratings.Average(rt => rt.Rating1) : 0,
                    IngredientCount = r.RecipeIngredients?.Count ?? 0,
                    StepCount = r.Steps?.Count ?? 0
                }).ToList();

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecipes = totalRecipes;
                ViewBag.TotalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipes");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách công thức.";
                return View(new List<RecipeApprovalListViewModel>());
            }
        }

        public async Task<IActionResult> Featured(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Recipes
                    .Include(r => r.User)
                    .Include(r => r.Favorites)
                    .Include(r => r.Ratings)
                    .Include(r => r.RecipeIngredients)
                    .Include(r => r.Steps)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(r => 
                        r.Title.ToLower().Contains(searchLower) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchLower)));
                }

                // Load all recipes first, then filter and sort in memory
                var allRecipes = await query.ToListAsync();

                // Filter for featured recipes
                // Consider recipes as featured if:
                // 1. High rating (>= 4.0) with at least 5 ratings
                // 2. Or high favorite count (>= 10 favorites)
                var featuredRecipes = allRecipes.Where(r =>
                {
                    var ratingCount = r.Ratings?.Count ?? 0;
                    var avgRating = r.Ratings != null && r.Ratings.Any() 
                        ? r.Ratings.Average(rt => rt.Rating1) 
                        : 0;
                    var favoriteCount = r.Favorites?.Count ?? 0;

                    return (ratingCount >= 5 && avgRating >= 4.0) || favoriteCount >= 10;
                }).ToList();

                var totalRecipes = featuredRecipes.Count;

                // Sort by combined score (rating * 0.6 + favorites * 0.4)
                var sortedRecipes = featuredRecipes
                    .OrderByDescending(r =>
                    {
                        var avgRating = r.Ratings != null && r.Ratings.Any()
                            ? r.Ratings.Average(rt => rt.Rating1)
                            : 0;
                        var favoriteCount = r.Favorites?.Count ?? 0;
                        return avgRating * 0.6 + favoriteCount * 0.4;
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModels = sortedRecipes.Select(r => new RecipeApprovalListViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    AuthorName = r.User?.UserName ?? r.UserId,
                    AuthorId = r.UserId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    FavoriteCount = r.Favorites?.Count ?? 0,
                    AvgRating = r.Ratings != null && r.Ratings.Any() ? r.Ratings.Average(rt => rt.Rating1) : 0,
                    IngredientCount = r.RecipeIngredients?.Count ?? 0,
                    StepCount = r.Steps?.Count ?? 0
                }).ToList();

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecipes = totalRecipes;
                ViewBag.TotalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured recipes");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách công thức nổi bật.";
                return View(new List<RecipeApprovalListViewModel>());
            }
        }

        public async Task<IActionResult> Pending(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Recipes
                    .Include(r => r.User)
                    .Include(r => r.Favorites)
                    .Include(r => r.Ratings)
                    .Include(r => r.RecipeIngredients)
                    .Include(r => r.Steps)
                    .AsQueryable();

                // Filter for pending recipes
                // For now, consider recipes as pending if:
                // 1. CreatedAt is within last 7 days (new recipes)
                // 2. Or UpdatedAt is null (not updated since creation)
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                query = query.Where(r => 
                    (r.CreatedAt.HasValue && r.CreatedAt.Value >= sevenDaysAgo) ||
                    (r.CreatedAt.HasValue && !r.UpdatedAt.HasValue));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(r => 
                        r.Title.ToLower().Contains(searchLower) ||
                        (r.Description != null && r.Description.ToLower().Contains(searchLower)));
                }

                var totalRecipes = await query.CountAsync();
                var recipes = await query
                    .OrderByDescending(r => r.CreatedAt ?? DateTime.MinValue)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = recipes.Select(r => new RecipeApprovalListViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    AuthorName = r.User?.UserName ?? r.UserId,
                    AuthorId = r.UserId,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    FavoriteCount = r.Favorites?.Count ?? 0,
                    AvgRating = r.Ratings != null && r.Ratings.Any() ? r.Ratings.Average(rt => rt.Rating1) : 0,
                    IngredientCount = r.RecipeIngredients?.Count ?? 0,
                    StepCount = r.Steps?.Count ?? 0
                }).ToList();

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecipes = totalRecipes;
                ViewBag.TotalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending recipes");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách công thức chờ duyệt.";
                return View(new List<RecipeApprovalListViewModel>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID công thức không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                {
                    TempData["Error"] = "Không tìm thấy công thức.";
                    return RedirectToAction(nameof(Index));
                }

                var ratings = await _ratingService.GetRatingsForRecipeAsync(id);
                var favoritesForRecipe = await _dbContext.Favorites
                    .Where(f => f.RecipeId == id)
                    .CountAsync();

                // Ensure navigation properties are loaded
                var recipeWithNav = await _dbContext.Recipes
                    .Include(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                    .Include(r => r.RecipeTags)
                        .ThenInclude(rt => rt.Tag)
                    .Include(r => r.Steps)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipeWithNav == null)
                {
                    TempData["Error"] = "Không tìm thấy công thức.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new RecipeApprovalDetailsViewModel
                {
                    Id = recipeWithNav.Id,
                    Title = recipeWithNav.Title ?? string.Empty,
                    Description = recipeWithNav.Description,
                    ImageUrl = recipeWithNav.ImageUrl,
                    AuthorName = recipeWithNav.User?.UserName ?? recipeWithNav.UserId ?? string.Empty,
                    AuthorId = recipeWithNav.UserId ?? string.Empty,
                    CreatedAt = recipeWithNav.CreatedAt,
                    UpdatedAt = recipeWithNav.UpdatedAt,
                    Servings = recipeWithNav.Servings,
                    PrepTimeMinutes = recipeWithNav.PrepTimeMinutes,
                    CookTimeMinutes = recipeWithNav.CookTimeMinutes,
                    FavoriteCount = favoritesForRecipe,
                    AvgRating = ratings.Any() ? ratings.Average(r => r.Rating1) : 0,
                    RatingCount = ratings.Count,
                    Ingredients = recipeWithNav.RecipeIngredients?
                        .Select(ri => $"{ri.Ingredient?.Name ?? ri.IngredientId ?? "N/A"} - {ri.Quantity} {ri.Unit ?? ""}")
                        .ToList() ?? new List<string>(),
                    Steps = recipeWithNav.Steps?
                        .OrderBy(s => s.StepNumber)
                        .Select(s => $"Bước {s.StepNumber}: {s.Instruction ?? ""}")
                        .ToList() ?? new List<string>(),
                    Tags = recipeWithNav.RecipeTags?
                        .Select(rt => rt.Tag?.Name ?? rt.TagId ?? "N/A")
                        .ToList() ?? new List<string>()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipe details {RecipeId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin công thức: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID công thức không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                {
                    TempData["Error"] = "Không tìm thấy công thức.";
                    return RedirectToAction(nameof(Index));
                }

                // If you have an approval status field, update it here
                // For now, just log the action
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                _logger.LogInformation("Recipe {RecipeId} approved by {AdminId}", id, adminId);
                
                TempData["Success"] = "Công thức đã được duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving recipe {RecipeId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi duyệt công thức: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string? reason)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID công thức không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null)
                {
                    TempData["Error"] = "Không tìm thấy công thức.";
                    return RedirectToAction(nameof(Index));
                }

                // If you have an approval status field, update it here with rejection reason
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                _logger.LogInformation("Recipe {RecipeId} rejected by {AdminId}, reason: {Reason}", 
                    id, adminId, reason ?? "No reason provided");
                
                TempData["Success"] = "Công thức đã bị từ chối.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting recipe {RecipeId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi từ chối công thức: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID công thức không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _recipeService.DeleteRecipeAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa công thức thành công.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy công thức để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe {RecipeId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa công thức: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

