using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using DigitalArena.DBContext;
using DigitalArena.Models; 

namespace DigitalArena.Controllers
{
    public class HomeController : Controller
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

        public ActionResult Index()
        {
            if (User.IsInRole("ADMIN"))
                return RedirectToAction("Index", "AdminDashboard");

            return RedirectToAction("LandingPage", "Home");
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

        [AllowAnonymous]
        public ActionResult LandingPage()
        {
            var landing = db.LandingPage.Include(lp => lp.Product).FirstOrDefault();

            if (landing == null) return HttpNotFound();

            var model = new LandingPageViewModel
            {
                ProductId = landing.ProductId,
                Headline = landing.Headline,
                SubHeadline = landing.SubHeadline,
                HeroImageUrl = Url.Content($"~/Assets/landing_page/{landing.Product.Thumbnail}")
            };
            return View(model);
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
