using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;

namespace DigitalArena.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();

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
                user.LastLoginAt = DateTime.Now;
                _dbContext.SaveChanges();

                FormsAuthentication.SetAuthCookie(model.Username, true);

                if(user.Role == "SELLER"){
                    return RedirectToAction("Index", "SellerDashboard");
                }else if(user.Role == "ADMIN"){
                    return RedirectToAction("Index", "AdminDashboard");
                }else{
                    return RedirectToAction("Index", "Home");
                }
            }

            if (!user.IsActive) {
                ViewBag.Message = "User is not Active. Contact Admin";
                ViewBag.IsSuccess = false;
                return View(model);
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


        
        
        public ActionResult ForgotPassword()
        {
            if (Request.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string Identifier)
        {
            if (string.IsNullOrWhiteSpace(Identifier))
            {
                ViewBag.Message = "Please enter a valid username or email.";
                ViewBag.IsSuccess = false;
                return View();
            }

            var user = _dbContext.User.FirstOrDefault(u =>
                u.Username.ToLower() == Identifier.ToLower() ||
                u.Email.ToLower() == Identifier.ToLower());

            if (user != null)
            {
                // Generate 6-digit OTP
                string otp = new Random().Next(100000, 999999).ToString();

                // Store OTP in session (valid for 2 minutes)
                Session["ResetOtp"] = otp;
                Session["OtpUserId"] = user.UserId;
                Session["OtpExpiry"] = DateTime.Now.AddMinutes(2);

                // Prepare SMS
                string message = $"Dear {user.FullName.Split(' ')[0]},\nYour Digital Arena Reset Password OTP is {otp}.\nThis code will expire in 2 minutes.\n\nPlease do not share this OTP with anyone.";
                var smsService = new SmsService();
                string smsResponse = smsService.SendSms(user.Phone, message);

                if (smsResponse.Contains("\"response_code\":202"))
                {
                    ViewBag.Message = "An OTP has been sent to your registered phone number.";
                    ViewBag.IsSuccess = true;
                    return View("OTPVerification");
                }
                else
                {
                    ViewBag.Message = "Failed to send OTP. Response: " + smsResponse;
                    ViewBag.IsSuccess = false;
                }
            }
            else
            {
                ViewBag.Message = "No account found with that username or email.";
                ViewBag.IsSuccess = false;
            }

            return View();
        }




        public ActionResult OTPVerification()
        {
            // Check if session contains a 6-digit OTP
            if (Session["ResetOtp"] == null || !Regex.IsMatch(Session["ResetOtp"].ToString(), @"^\d{6}$"))
            {
                // Redirect to ForgotPassword action if no valid OTP is found
                return RedirectToAction("ForgotPassword", "Auth");
            }

            return View();
        }
        [HttpPost]
        public ActionResult OTPVerification(string[] OtpDigits)
        {
            string enteredOtp = string.Join("", OtpDigits);

            // Check if OTP, UserId and Expiry exist in session
            if (Session["ResetOtp"] == null || Session["OtpUserId"] == null || Session["OtpExpiry"] == null)
            {
                ViewBag.Message = "Your session has expired. Please request a new OTP.";
                ViewBag.IsSuccess = false;
                return RedirectToAction("ForgotPassword");
            }

            string storedOtp = Session["ResetOtp"].ToString();
            DateTime expiry = (DateTime)Session["OtpExpiry"];

            // Check expiry
            if (DateTime.Now > expiry)
            {
                // Clear expired data
                Session.Remove("ResetOtp");
                Session.Remove("OtpUserId");
                Session.Remove("OtpExpiry");

                ViewBag.Message = "OTP has expired. Please request a new one.";
                ViewBag.IsSuccess = false;
                return RedirectToAction("ForgotPassword");
            }

            // Check OTP
            if (enteredOtp != storedOtp)
            {
                ViewBag.Message = "Invalid OTP. Please try again.";
                ViewBag.IsSuccess = false;
                return View();
            }

            // OTP is valid
            int userId = (int)Session["OtpUserId"];
            Session.Remove("ResetOtp");
            Session.Remove("OtpUserId");
            Session.Remove("OtpExpiry");

            Session["AllowPasswordResetUserId"] = userId;

            return RedirectToAction("ResetPassword");
        }




        public ActionResult ResetPassword()
        {
            // Check if user allowed to reset password
            if (Session["AllowPasswordResetUserId"] == null)
            {
                return RedirectToAction("ForgotPassword", "Auth");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.Message = "Please fill in all fields.";
                ViewBag.IsSuccess = false;
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                ViewBag.IsSuccess = false;
                return View();
            }

            if (Session["AllowPasswordResetUserId"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            int userId = (int)Session["AllowPasswordResetUserId"];

            var user = _dbContext.User.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                ViewBag.Message = "User not found.";
                ViewBag.IsSuccess = false;
                return View();
            }

            user.Password = NewPassword;

            _dbContext.SaveChanges();

            // Clear session after successful reset
            Session.Remove("AllowPasswordResetUserId");

            ViewBag.Message = "Your password has been successfully reset.";
            ViewBag.IsSuccess = true;

            return RedirectToAction("Login");
        }




        public ActionResult Logout()
        {
            if (!Request.IsAuthenticated) return RedirectToAction("Login", "Auth");

            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}