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
            ViewBag.Message = Resources.Resources.ApplicationName;

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
            ViewBag.Message = Resources.Resources.ApplicationName;

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = Resources.Resources.ApplicationName;

            return View();
        }
    }
}