using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodVault.ViewComponents;

public sealed class FavoriteCountViewComponent : ViewComponent
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteCountViewComponent(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        int count = 0;
        if (!string.IsNullOrEmpty(userId))
        {
            var recipes = await _favoriteService.GetUserFavoriteRecipesAsync(userId);
            count = recipes.Count;
        }
        return View(count);
    }
}











