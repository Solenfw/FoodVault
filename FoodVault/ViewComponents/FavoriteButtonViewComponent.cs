using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.ViewComponents;

public sealed class FavoriteButtonViewComponent : ViewComponent
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteButtonViewComponent(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string recipeId)
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isFav = string.IsNullOrEmpty(userId) ? false : await _favoriteService.IsFavoriteAsync(userId, recipeId);
        var vm = new FavoriteButtonViewModel { RecipeId = recipeId, IsFavorite = isFav };
        return View(vm);
    }
}














