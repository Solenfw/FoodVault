using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Models.ViewModels;

namespace FoodVault.ViewComponents
{
	public class SearchBoxViewComponent : ViewComponent
	{
		public Task<IViewComponentResult> InvokeAsync(SearchBoxViewModel? model)
		{
			var safe = model ?? new SearchBoxViewModel();
			return Task.FromResult<IViewComponentResult>(View("~/Views/Shared/Components/SearchBox/Default.cshtml", safe));
		}
	}
}








