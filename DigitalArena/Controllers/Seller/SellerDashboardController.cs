using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    [Authorize(Roles = "SELLER")]
    public class SellerDashboardController : Controller
    {
        // GET: SellerDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}