using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Route("admin")]
    public class AdminDashboardController : Controller
    {

        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();

        [Route("dashboard")]
        public ActionResult Index()
        {
            // Dummy data for dashboard stats
            ViewBag.TotalBuyers = 256;
            ViewBag.OrdersToday = 34;
            ViewBag.Revenue = 12540.75m;
            ViewBag.NewReviews = 12;

            // Dummy recent orders list
            ViewBag.RecentOrders = new List<dynamic>
            {
                new { OrderId = 101, BuyerName = "John Doe", TotalAmount = 150.00m, Status = "Completed", OrderDate = DateTime.Today.AddDays(-1) },
                new { OrderId = 102, BuyerName = "Jane Smith", TotalAmount = 249.99m, Status = "Pending", OrderDate = DateTime.Today.AddDays(-2) },
                new { OrderId = 103, BuyerName = "Alice Johnson", TotalAmount = 89.50m, Status = "Completed", OrderDate = DateTime.Today.AddDays(-3) },
                new { OrderId = 104, BuyerName = "Bob Brown", TotalAmount = 300.00m, Status = "Pending", OrderDate = DateTime.Today.AddDays(-4) },
            };

            return View();
        }


        [Route("manage-buyers", Name = "ManageBuyersRoute")]
        public ActionResult ManageBuyers(int page = 1, int pageSize = 10, string search = "", string status = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.User.Where(u => u.Role == "Buyer");

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search));
            }

            if (status == "active")
                query = query.Where(u => u.IsActive);
            else if (status == "inactive")
                query = query.Where(u => !u.IsActive);

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            int totalBuyers = query.Count();

            var buyers = query
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    ProfileImage = u.ProfileImage,
                    FullName = u.FullName,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt ?? DateTime.MinValue
                })
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBuyers / pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_BuyersTable", buyers);

            return View(buyers);
        }



    }
}