using FoodVault.Models.ViewModels;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.ViewComponents;

public sealed class RecentRecipesViewComponent : ViewComponent
{
    private readonly IRecipeService _recipeService;

    public RecentRecipesViewComponent(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string userId, int take = 5)
    {
        var recipes = await _recipeService.GetUserRecipesAsync(userId);
        var vm = new RecentRecipesViewModel
        {
            Recipes = recipes
                .OrderByDescending(r => r.UpdatedAt)
                .Take(take)
                .Select(r => new RecipeListItemViewModel { Id = r.Id, Title = r.Title, Description = r.Description, UpdatedAt = r.UpdatedAt })
                .ToList()
        };
        return View(vm);
    }
}















