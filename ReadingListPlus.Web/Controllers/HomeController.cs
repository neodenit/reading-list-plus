using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReadingListPlus.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Reading List Plus";

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
            ViewBag.Message = "Reading List Plus";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Reading List Plus";

            return View();
        }
    }
}