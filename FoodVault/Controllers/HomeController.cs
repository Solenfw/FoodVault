using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Services.Interfaces;

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
		public async Task<IActionResult> Index()
		{
			var vm = await _homeService.GetHomeAsync();
			return View(vm);
		}
	}
}



