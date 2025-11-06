using FoodVault.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodVault.Controllers
{
    [AllowAnonymous]
    public class PagesController : Controller
    {
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            TempData["Success"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!";
            return RedirectToAction(nameof(Contact));
        }

        [HttpGet]
        public IActionResult Careers()
        {
            return View(new CareerApplicationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Careers(CareerApplicationViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            TempData["Success"] = "Ứng tuyển đã được gửi. Cảm ơn bạn!";
            return RedirectToAction(nameof(Careers));
        }
    }
}


