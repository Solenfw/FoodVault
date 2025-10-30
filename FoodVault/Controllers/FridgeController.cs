using FoodVault.Models.Entities;
using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.Controllers
{
    [Authorize]
    public class FridgeController : Controller
    {
        private readonly IFridgeService _fridgeService;
        private readonly ILogger<FridgeController> _logger;

        public FridgeController(IFridgeService fridgeService, ILogger<FridgeController> logger)
        {
            _fridgeService = fridgeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var fridges = await _fridgeService.GetUserFridgesAsync(userId);
            var allIngredients = await _fridgeService.GetAllIngredientsAsync();
            ViewBag.AllIngredients = allIngredients.OrderBy(i => i.Name).ToList();

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
                var entity = new FridgeIngredient
                {
                    FridgeId = vm.FridgeId,
                    IngredientId = vm.IngredientId,
                    Quantity =(double) vm.Quantity,
                    Unit = vm.Unit
                };
                await _fridgeService.AddIngredientAsync(entity);
                TempData["Success"] = "Item added.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to fridge {FridgeId}", vm.FridgeId);
                TempData["Error"] = "Failed to add item.";
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
    }
}










