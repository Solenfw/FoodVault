using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodVault.Models;
using System.Diagnostics;

namespace FoodVault.Controllers
{
    /// <summary>
    /// Controller xử lý các lỗi và trang truy cập bị từ chối
    /// </summary>
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        /// <summary>
        /// Hiển thị trang Access Denied (403)
        /// </summary>
        /// <returns>View AccessDenied</returns>
        [Route("Error/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        /// <summary>
        /// Xử lý các lỗi HTTP khác nhau
        /// </summary>
        /// <param name="statusCode">Mã lỗi HTTP</param>
        /// <returns>View tương ứng với mã lỗi</returns>
        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            // Tạo ErrorViewModel để tránh lỗi NullReference
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (statusCode == 403)
            {
                return View("~/Views/Shared/AccessDenied.cshtml");
            }

            // Truyền model vào View
            return View("Error", errorViewModel);
        }

        /// <summary>
        /// Trang lỗi chung
        /// </summary>
        [Route("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(errorViewModel);
        }
    }
}