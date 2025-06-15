using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI;

namespace DigitalArena.Controllers
{
    public class AuthController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            ViewBag.Username = Username;
            ViewBag.Password = Password;

            if (Username == "expecteduser" && Password == "expectedpass")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}