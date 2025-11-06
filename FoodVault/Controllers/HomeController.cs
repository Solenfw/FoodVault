using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FoodVault.Controllers
{
	public class HomeController : Controller
	{
		private readonly IHomeService _homeService;

		public HomeController(IHomeService homeService)
		{
			_homeService = homeService;
		}

		[HttpGet]
        public async Task<IActionResult> Index([FromServices] IThemeService themeService)
		{
            var vm = await _homeService.GetHomeAsync();
            var theme = await themeService.ResolveThemeAsync(User, Request);
            ViewData["UserTheme"] = theme;
			return View(vm);
		}
	}
}



