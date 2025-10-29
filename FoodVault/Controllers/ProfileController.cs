using FoodVault.Models;
using FoodVault.Models.ViewModels;
using FoodVault.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodVault.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly FileUploadService _fileUploadService; 

        public ProfileController(UserManager<User> userManager, FileUploadService fileUploadService)
        {
            _userManager = userManager;
            _fileUploadService = fileUploadService; 
        }

        [HttpGet] 
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Create a ViewModel and populate it with the user's current data
            var viewModel = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Update text fields
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;

            // Check if a new avatar image was uploaded
            if (viewModel.ProfilePicture != null)
            {
                // Use the service to save the file
                string filePath = await _fileUploadService.UploadFileAsync(viewModel.ProfilePicture, "profilepictures");

                // Save the returned path to the user
                user.ProfilePictureUrl = filePath;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        [Route("Profile")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }
    }
}
