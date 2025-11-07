using FoodVault.Models.Entities;
using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class FridgeController : Controller
    {
        private readonly IFridgeService _fridgeService;
        private readonly ILogger<FridgeController> _logger;
        private readonly FoodVault.Models.Data.FoodVaultDbContext _dbContext;

        public FridgeController(IFridgeService fridgeService, ILogger<FridgeController> logger, FoodVault.Models.Data.FoodVaultDbContext dbContext)
        {
            _fridgeService = fridgeService;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var fridges = await _fridgeService.GetUserFridgesAsync(userId);

            var fridgeVMs = new List<FridgeWithIngredientsViewModel>();
            foreach (var fridge in fridges) {
                var ingredients = await _fridgeService.GetFridgeIngredientsAsync(fridge.Id);
                var ingredientVMs = ingredients.Select(i => new FridgeIngredientViewModel {
                    Id = i.Id,
                    IngredientName = i.Ingredient?.Name ?? i.IngredientId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    ExpirationDate = i.ExpirationDate,
                    Calories = i.Quantity * (i.Ingredient?.DefaultCalories ?? 0),
                    Protein = i.Quantity * (i.Ingredient?.DefaultProtein ?? 0),
                    Fat = i.Quantity * (i.Ingredient?.DefaultFat ?? 0),
                    Carbs = i.Quantity * (i.Ingredient?.DefaultCarbs ?? 0),
                }).ToList();
                fridgeVMs.Add(new FridgeWithIngredientsViewModel {
                    Id = fridge.Id,
                    Name = fridge.Name,
                    Ingredients = ingredientVMs
                });
            }
            return View(fridgeVMs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(AddFridgeItemViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
            try
            {
                // Tìm hoặc tạo ingredient từ tên
                var ingredientName = vm.IngredientName.Trim();
                if (string.IsNullOrWhiteSpace(ingredientName))
                {
                    TempData["Error"] = "Tên nguyên liệu không được để trống.";
                    return RedirectToAction(nameof(Index));
                }

                // Tìm ingredient đã tồn tại (không phân biệt hoa thường)
                var existingIngredient = await _dbContext.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == ingredientName.ToLower());

                string ingredientId;
                if (existingIngredient != null)
                {
                    ingredientId = existingIngredient.Id;
                }
                else
                {
                    // Tạo ingredient mới nếu chưa tồn tại
                    var newIngredient = new Ingredient
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = ingredientName
                    };
                    await _dbContext.Ingredients.AddAsync(newIngredient);
                    await _dbContext.SaveChangesAsync();
                    ingredientId = newIngredient.Id;
                }

                // Tính ExpirationDate nếu DaysToExpire được cung cấp và ExpirationDate chưa có
                DateOnly? expiration = vm.ExpirationDate;
                if (!expiration.HasValue && vm.DaysToExpire.HasValue && vm.DaysToExpire.Value > 0)
                {
                    expiration = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(vm.DaysToExpire.Value));
                }
                
                var entity = new FridgeIngredient
                {
                    FridgeId = vm.FridgeId,
                    IngredientId = ingredientId,
                    Quantity = (double)(vm.Quantity ?? 1),
                    Unit = vm.Unit,
                    ExpirationDate = expiration
                };
                await _fridgeService.AddIngredientAsync(entity);
                TempData["Success"] = "Đã thêm nguyên liệu thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to fridge {FridgeId}", vm.FridgeId);
                TempData["Error"] = "Có lỗi xảy ra khi thêm nguyên liệu.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(RemoveFridgeItemViewModel vm)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
            try
            {
                await _fridgeService.RemoveIngredientAsync(vm.FridgeIngredientId);
                TempData["Success"] = "Item removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove item {FridgeIngredientId}", vm.FridgeIngredientId);
                TempData["Error"] = "Failed to remove item.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFridge(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Tên tủ lạnh không được bỏ trống.";
                return RedirectToAction(nameof(Index));
            }
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var entity = new FoodVault.Models.Entities.Fridge
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                UserId = userId ?? ""
            };
            await _fridgeService.CreateFridgeAsync(entity);
            TempData["Success"] = "Tạo tủ lạnh mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFridge(string fridgeId)
        {
            if (string.IsNullOrEmpty(fridgeId))
            {
                TempData["Error"] = "Không tìm thấy tủ lạnh để xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Bạn cần đăng nhập để xóa tủ lạnh.";
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra xem tủ lạnh có thuộc về user không
                var fridges = await _fridgeService.GetUserFridgesAsync(userId);
                if (!fridges.Any(f => f.Id == fridgeId))
                {
                    TempData["Error"] = "Bạn không có quyền xóa tủ lạnh này.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _fridgeService.DeleteFridgeAsync(fridgeId);
                if (result)
                {
                    TempData["Success"] = "Xóa tủ lạnh thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy tủ lạnh để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete fridge {FridgeId}", fridgeId);
                TempData["Error"] = "Có lỗi xảy ra khi xóa tủ lạnh.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}










