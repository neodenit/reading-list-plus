using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReadingListPlus.Web.Core.Models;

namespace ReadingListPlus.Web.Core.Controllers
{
    public class HomeController : Controller
    {
        public const string Name = "Home";

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(DecksController.Index), DecksController.Name);
            }
            else
            {
                return View();
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
