using FoodVault.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.ViewComponents;

public sealed class UserProfileViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(UserProfileSidebarViewModel model)
    {
        return View(model);
    }
}











