using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.UI;

namespace DigitalArena.Controllers
{
    public class AuthController : Controller
    {

        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();
        // GET: Login
        public ActionResult Login()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");
            
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid form submission.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            var user = _dbContext.User.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            // User Authenticated
            if (user != null && user.IsActive)
            {
                Session["UserId"] = user.UserId;
                Session["Role"] = user.Role;
                user.LastLoginAt = DateTime.Now;
                _dbContext.SaveChanges();

                FormsAuthentication.SetAuthCookie(model.Username, true);

                if(user.Role == "SELLER")
                {
                    return RedirectToAction("Index", "SellerDashboard");
                }else if(user.Role == "ADMIN")
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Message = "Invalid username or password.";
            ViewBag.IsSuccess = false;
            return View(model);
        }

        public ActionResult Register()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult Register(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Invalid form submission.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            // Check for existing email or username
            var existingUser = _dbContext.User.FirstOrDefault(u => u.Email == model.Email || u.Username == model.Username);
            if (existingUser != null)
            {
                ViewBag.Message = "Email or Username already exists.";
                ViewBag.IsSuccess = false;
                return View();
            }

            // Create new user
            User newUser = new User
            {
                Email = model.Email,
                Username = model.Username,
                Phone = model.Phone,
                FullName = model.FullName,
                Password = model.Password,

                Role = "BUYER",
                IsActive = true,
                CreatedAt = DateTime.Now,
            };

            // Save user to DB
            _dbContext.User.Add(newUser);
            _dbContext.SaveChanges();

            // Create cart for this user
            var newCart = new Cart
            {
                UserId = newUser.UserId
            };
            _dbContext.Cart.Add(newCart);
            _dbContext.SaveChanges();


            ViewBag.Message = "Registration successful!";
            ViewBag.IsSuccess = true;
            return View("Login");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}