using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace DigitalArena.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();
        public ActionResult Index()
        {
            if (User.IsInRole("ADMIN")) return RedirectToAction("Index", "AdminDashboard");
            if (User.IsInRole("SELLER")) return RedirectToAction("Index", "SellerDashboard");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult LandingPage()
        {
            var landingContent = db.LandingPage
                .Include(l => l.Product)
                .FirstOrDefault(); // Or filter as needed

            return View(landingContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}