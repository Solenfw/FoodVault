using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.ViewComponents
{
	public class RecipeCardViewComponent : ViewComponent
	{
		public Task<IViewComponentResult> InvokeAsync(object model)
		{
			return Task.FromResult<IViewComponentResult>(View("~/Views/Shared/_RecipeCard.cshtml", model));
		}
	}
}



