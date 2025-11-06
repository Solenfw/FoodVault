using FoodVault.Areas.Admin.ViewModels;
using FoodVault.Models.Data;
using FoodVault.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodVault.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class IngredientsController : Controller
    {
        private readonly FoodVaultDbContext _dbContext;
        private readonly ILogger<IngredientsController> _logger;

        public IngredientsController(FoodVaultDbContext dbContext, ILogger<IngredientsController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _dbContext.Ingredients
                    .Include(i => i.RecipeIngredients)
                    .Include(i => i.FridgeIngredients)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(i => i.Name.ToLower().Contains(searchLower));
                }

                var totalIngredients = await query.CountAsync();
                var ingredients = await query
                    .OrderBy(i => i.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModels = ingredients.Select(i => new IngredientListViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    DefaultUnit = i.DefaultUnit,
                    DefaultCalories = i.DefaultCalories,
                    DefaultProtein = i.DefaultProtein,
                    DefaultFat = i.DefaultFat,
                    DefaultCarbs = i.DefaultCarbs,
                    ImageUrl = i.ImageUrl,
                    RecipeCount = i.RecipeIngredients?.Count ?? 0,
                    FridgeCount = i.FridgeIngredients?.Count ?? 0
                }).ToList();

                ViewBag.Search = search;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalIngredients = totalIngredients;
                ViewBag.TotalPages = (int)Math.Ceiling(totalIngredients / (double)pageSize);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ingredients");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách nguyên liệu.";
                return View(new List<IngredientListViewModel>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateIngredientViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIngredientViewModel vm, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                // Check if ingredient name already exists
                var exists = await _dbContext.Ingredients.AnyAsync(i => i.Name.ToLower() == vm.Name.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(vm.Name), "Nguyên liệu này đã tồn tại.");
                    return View(vm);
                }

                var ingredient = new Ingredient
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = vm.Name,
                    DefaultUnit = vm.DefaultUnit,
                    DefaultCalories = vm.DefaultCalories,
                    DefaultProtein = vm.DefaultProtein,
                    DefaultFat = vm.DefaultFat,
                    DefaultCarbs = vm.DefaultCarbs,
                    ImageUrl = vm.ImageUrl
                };

                // Handle image upload if provided
                if (Image != null && Image.Length > 0)
                {
                    // TODO: Upload image using ImageService
                    // For now, just save URL if provided
                    ingredient.ImageUrl = vm.ImageUrl;
                }

                await _dbContext.Ingredients.AddAsync(ingredient);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Tạo nguyên liệu thành công.";
                return RedirectToAction(nameof(Details), new { id = ingredient.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ingredient");
                TempData["Error"] = "Có lỗi xảy ra khi tạo nguyên liệu.";
                return View(vm);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID nguyên liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ingredient = await _dbContext.Ingredients
                    .Include(i => i.RecipeIngredients)
                    .ThenInclude(ri => ri.Recipe)
                    .Include(i => i.FridgeIngredients)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (ingredient == null)
                {
                    TempData["Error"] = "Không tìm thấy nguyên liệu.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new IngredientDetailsViewModel
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name,
                    DefaultUnit = ingredient.DefaultUnit,
                    DefaultCalories = ingredient.DefaultCalories,
                    DefaultProtein = ingredient.DefaultProtein,
                    DefaultFat = ingredient.DefaultFat,
                    DefaultCarbs = ingredient.DefaultCarbs,
                    ImageUrl = ingredient.ImageUrl,
                    RecipeCount = ingredient.RecipeIngredients?.Count ?? 0,
                    FridgeCount = ingredient.FridgeIngredients?.Count ?? 0
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ingredient details {IngredientId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin nguyên liệu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID nguyên liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ingredient = await _dbContext.Ingredients.FindAsync(id);
                if (ingredient == null)
                {
                    TempData["Error"] = "Không tìm thấy nguyên liệu.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = new EditIngredientViewModel
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name,
                    DefaultUnit = ingredient.DefaultUnit,
                    DefaultCalories = ingredient.DefaultCalories,
                    DefaultProtein = ingredient.DefaultProtein,
                    DefaultFat = ingredient.DefaultFat,
                    DefaultCarbs = ingredient.DefaultCarbs,
                    ImageUrl = ingredient.ImageUrl
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ingredient for edit {IngredientId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin nguyên liệu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditIngredientViewModel vm, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                var ingredient = await _dbContext.Ingredients.FindAsync(vm.Id);
                if (ingredient == null)
                {
                    TempData["Error"] = "Không tìm thấy nguyên liệu.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if name changed and conflicts with existing
                if (ingredient.Name.ToLower() != vm.Name.ToLower())
                {
                    var exists = await _dbContext.Ingredients.AnyAsync(i => i.Name.ToLower() == vm.Name.ToLower() && i.Id != vm.Id);
                    if (exists)
                    {
                        ModelState.AddModelError(nameof(vm.Name), "Nguyên liệu này đã tồn tại.");
                        return View(vm);
                    }
                }

                ingredient.Name = vm.Name;
                ingredient.DefaultUnit = vm.DefaultUnit;
                ingredient.DefaultCalories = vm.DefaultCalories;
                ingredient.DefaultProtein = vm.DefaultProtein;
                ingredient.DefaultFat = vm.DefaultFat;
                ingredient.DefaultCarbs = vm.DefaultCarbs;

                // Handle image upload if provided
                if (Image != null && Image.Length > 0)
                {
                    // TODO: Upload image using ImageService
                    ingredient.ImageUrl = vm.ImageUrl;
                }
                else if (!string.IsNullOrEmpty(vm.ImageUrl))
                {
                    ingredient.ImageUrl = vm.ImageUrl;
                }

                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Cập nhật nguyên liệu thành công.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient {IngredientId}", vm.Id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật nguyên liệu.";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID nguyên liệu không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var ingredient = await _dbContext.Ingredients
                    .Include(i => i.RecipeIngredients)
                    .Include(i => i.FridgeIngredients)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (ingredient == null)
                {
                    TempData["Error"] = "Không tìm thấy nguyên liệu cần xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if ingredient is used in recipes or fridges
                if ((ingredient.RecipeIngredients?.Any() ?? false) || (ingredient.FridgeIngredients?.Any() ?? false))
                {
                    TempData["Error"] = "Không thể xóa nguyên liệu này vì nó đang được sử dụng trong công thức hoặc tủ lạnh.";
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.Ingredients.Remove(ingredient);
                await _dbContext.SaveChangesAsync();

                TempData["Success"] = "Xóa nguyên liệu thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient {IngredientId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa nguyên liệu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

