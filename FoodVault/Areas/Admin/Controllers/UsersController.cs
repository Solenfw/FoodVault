using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRecipeService _recipeService;
        private readonly IFavoriteService _favoriteService;
        private readonly FoodVaultDbContext _dbContext;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IRecipeService recipeService,
            IFavoriteService favoriteService,
            FoodVaultDbContext dbContext,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _recipeService = recipeService;
            _favoriteService = favoriteService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(u => 
                        u.UserName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower));
                }

                var totalUsers = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt ?? DateTime.MinValue)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = new List<UserListViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var recipes = await _recipeService.GetUserRecipesAsync(user.Id);
                    var favorites = await _favoriteService.GetUserFavoriteRecipesAsync(user.Id);

                    viewModels.Add(new UserListViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        PhoneNumber = user.PhoneNumber,
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd,
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt,
                        RecipeCount = recipes.Count,
                        FavoriteCount = favorites.Count
                    });
                }

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách người dùng.";
                return View(new List<UserListViewModel>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var recipes = await _recipeService.GetUserRecipesAsync(user.Id);
                var favorites = await _favoriteService.GetUserFavoriteRecipesAsync(user.Id);

                var vm = new UserDetailsViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    RecipeCount = recipes.Count,
                    FavoriteCount = favorites.Count,
                    RatingCount = 0 // TODO: Add rating count if needed
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

                var vm = new EditUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    SelectedRoles = roles.ToList(),
                    AvailableRoles = allRoles
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(vm);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(vm.Id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                // Update basic info
                user.UserName = vm.UserName;
                user.Email = vm.Email;
                user.PhoneNumber = vm.PhoneNumber;
                user.EmailConfirmed = vm.EmailConfirmed;
                user.LockoutEnabled = vm.LockoutEnabled;
                user.LockoutEnd = vm.LockoutEnd;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    vm.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                    return View(vm);
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(vm.SelectedRoles).ToList();
                var rolesToAdd = vm.SelectedRoles.Except(currentRoles).ToList();

                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", vm.Id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin người dùng.";
                vm.AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Don't allow deleting yourself
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user.Id == currentUserId)
                {
                    TempData["Error"] = "Bạn không thể xóa chính tài khoản của mình.";
                    return RedirectToAction(nameof(Index));
                }

                // Delete all related data before deleting the user
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Get all recipes for this user
                    var userRecipes = await _dbContext.Recipes
                        .Where(r => r.UserId == id)
                        .ToListAsync();

                    // Delete all recipe-related data
                    foreach (var recipe in userRecipes)
                    {
                        // Delete recipe ingredients
                        var recipeIngredients = await _dbContext.RecipeIngredients
                            .Where(ri => ri.RecipeId == recipe.Id)
                            .ToListAsync();
                        _dbContext.RecipeIngredients.RemoveRange(recipeIngredients);

                        // Delete recipe tags
                        var recipeTags = await _dbContext.RecipeTags
                            .Where(rt => rt.RecipeId == recipe.Id)
                            .ToListAsync();
                        _dbContext.RecipeTags.RemoveRange(recipeTags);

                        // Delete steps
                        var steps = await _dbContext.Steps
                            .Where(s => s.RecipeId == recipe.Id)
                            .ToListAsync();
                        _dbContext.Steps.RemoveRange(steps);

                        // Delete favorites for this recipe
                        var recipeFavorites = await _dbContext.Favorites
                            .Where(f => f.RecipeId == recipe.Id)
                            .ToListAsync();
                        _dbContext.Favorites.RemoveRange(recipeFavorites);

                        // Delete ratings for this recipe
                        var recipeRatings = await _dbContext.Ratings
                            .Where(r => r.RecipeId == recipe.Id)
                            .ToListAsync();
                        _dbContext.Ratings.RemoveRange(recipeRatings);
                    }

                    // Delete all recipes
                    _dbContext.Recipes.RemoveRange(userRecipes);

                    // Delete user's favorites
                    var userFavorites = await _dbContext.Favorites
                        .Where(f => f.UserId == id)
                        .ToListAsync();
                    _dbContext.Favorites.RemoveRange(userFavorites);

                    // Delete user's ratings
                    var userRatings = await _dbContext.Ratings
                        .Where(r => r.UserId == id)
                        .ToListAsync();
                    _dbContext.Ratings.RemoveRange(userRatings);

                    // Get all fridges for this user
                    var userFridges = await _dbContext.Fridges
                        .Where(f => f.UserId == id)
                        .ToListAsync();

                    // Delete fridge ingredients
                    foreach (var fridge in userFridges)
                    {
                        var fridgeIngredients = await _dbContext.FridgeIngredients
                            .Where(fi => fi.FridgeId == fridge.Id)
                            .ToListAsync();
                        _dbContext.FridgeIngredients.RemoveRange(fridgeIngredients);
                    }

                    // Delete all fridges
                    _dbContext.Fridges.RemoveRange(userFridges);

                    // Delete user preferences
                    var userPreferences = await _dbContext.UserPreferences
                        .Where(up => up.UserId == id)
                        .ToListAsync();
                    _dbContext.UserPreferences.RemoveRange(userPreferences);

                    // Delete user activities
                    var userActivities = await _dbContext.UserActivities
                        .Where(ua => ua.UserId == id)
                        .ToListAsync();
                    _dbContext.UserActivities.RemoveRange(userActivities);

                    // Save all deletions
                    await _dbContext.SaveChangesAsync();

                    // Now delete the user
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        await transaction.CommitAsync();
                        TempData["Success"] = "Xóa người dùng thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to delete user {UserId}: {Errors}", id, errors);
                        TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng: " + errors;
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during transaction while deleting user {UserId}", id);
                    TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu liên quan của người dùng: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, int? days = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                if (days.HasValue && days > 0)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(days.Value);
                }
                else
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Effectively permanent
                }

                user.LockoutEnabled = true;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "Khóa tài khoản thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi khóa tài khoản.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                user.LockoutEnd = null;
                user.LockoutEnabled = false;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "Mở khóa tài khoản thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi mở khóa tài khoản.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

