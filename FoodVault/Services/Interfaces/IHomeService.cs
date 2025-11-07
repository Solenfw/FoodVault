using System.Collections.Generic;
using System.Threading.Tasks;
using FoodVault.Models.ViewModels;

namespace FoodVault.Services.Interfaces
{
	public interface IHomeService
	{
		Task<HomeViewModel> GetHomeAsync();
	}
}



