using System;
using System.Linq;
using System.Web.Mvc;
using DigitalArena.Models;
using System.Collections.Generic;
using System.Data.Entity;
using DigitalArena.DBContext;

public class UserProfileController : Controller
{
    private DigitalArenaDBContext db = new DigitalArenaDBContext();

    public ActionResult UserProfile()
    {
        int userId = 5; // Replace with actual session/authenticated user ID

        var user = db.User.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return HttpNotFound();

        int totalPurchases = db.Permission.Count(p => p.UserId == userId && p.IsValid);

        // Fetch purchased products from valid permissions
        var purchasedProducts = db.Permission
            .Include(p => p.Product)
            .Where(p => p.UserId == userId && p.IsValid)
            .Select(p => p.Product)
            .ToList();

        // Map EF Product entity to ProductModel
        var productModels = purchasedProducts.Select(p => new ProductModel
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Thumbnail = p.Thumbnail,
            CreatedAt = p.CreatedAt
            // Add more fields if needed
        }).ToList();

        var model = new UserProfileViewModel
        {
            User = new UserModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                ProfileImage = user.ProfileImage,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt ?? DateTime.MinValue
            },
            TotalPurchases = totalPurchases,
            PurchasedProducts = productModels
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult ToggleMode(bool isSeller)
    {
        int userId = 5; // Replace with session/authenticated logic
        var user = db.User.FirstOrDefault(u => u.UserId == userId);

        if (user == null)
            return Json(new { success = false, message = "User not found" });

        user.Role = isSeller ? "Seller" : "Buyer";
        db.SaveChanges();

        return Json(new { success = true, role = user.Role });
    }
}
