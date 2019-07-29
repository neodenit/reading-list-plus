using System.Web.Mvc;
using ReadingListPlus.App_GlobalResources;

namespace ReadingListPlus.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = Resources.ApplicationName;

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Decks");
            }
            else
            {
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = Resources.ApplicationName;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = Resources.ApplicationName;

            return View();
        }
    }
}