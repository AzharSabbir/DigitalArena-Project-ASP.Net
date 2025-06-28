using System.Linq;
using System.Web.Mvc;
using DigitalArena.Models;
using DigitalArena.DBContext;
using System.Data.Entity;
using System;
using System.Web.Http;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using System.IO;

public class UserProfileController : Controller
{
    private DigitalArenaDBContext db = new DigitalArenaDBContext();

    public ActionResult UserProfile()
    {
        int userId = GetUserId();

        var user = db.User.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return HttpNotFound();

        var purchasedProducts = db.Permission
            .Where(p => p.UserId == userId && p.IsValid)
            .Include(p => p.Product.Category)
            .Include(p => p.Product.User)
            .Select(p => new ProductModel
            {
                ProductId = p.Product.ProductId,
                Name = p.Product.Name,
                Description = p.Product.Description,
                Price = p.Product.Price,
                Thumbnail = p.Product.Thumbnail,
                IsActive = p.Product.IsActive,
                CreatedAt = p.Product.CreatedAt,
                LikeCount = p.Product.LikeCount,
                UnlikeCount = p.Product.UnlikeCount,
                Status = p.Product.Status,
                CategoryId = p.Product.CategoryId,
                SellerId = p.Product.SellerId,
                ViewCount = p.Product.ViewCount,
                Category = new CategoryModel
                {
                    CategoryId = p.Product.Category.CategoryId,
                    Name = p.Product.Category.Name
                },
                User = new UserModel
                {
                    UserId = p.Product.User.UserId,
                    Username = p.Product.User.Username,
                    FullName = p.Product.User.FullName,
                    ProfileImage = p.Product.User.ProfileImage
                }
            })
            .ToList();

        var uploadedProducts = db.Product
            .Where(p => p.SellerId == userId)
            .Include(p => p.Category)
            .Include(p => p.User)
            .Select(p => new ProductModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Thumbnail = p.Thumbnail,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                LikeCount = p.LikeCount,
                UnlikeCount = p.UnlikeCount,
                Status = p.Status,
                CategoryId = p.CategoryId,
                SellerId = p.SellerId,
                ViewCount = p.ViewCount,
                Category = new CategoryModel
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name
                },
                User = new UserModel
                {
                    UserId = p.User.UserId,
                    Username = p.User.Username,
                    FullName = p.User.FullName,
                    ProfileImage = p.User.ProfileImage
                }
            })
            .ToList();

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
                LastLoginAt = user.LastLoginAt ?? DateTime.Now
            },
            TotalPurchases = purchasedProducts.Count,
            IsSellerMode = true,
            PurchasedProducts = purchasedProducts,
            UploadedProducts = uploadedProducts
        };
        ViewBag.Categories = db.Category
        .Select(c => new CategoryModel
        {
            CategoryId = c.CategoryId,
            Name = c.Name
        })
    .ToList();
        return View(model);
    }

    [HttpPost]
    public JsonResult ToggleMode([FromBody] RoleChangeRequest model)
    {
        var userId = GetUserId();
        var user = db.User.FirstOrDefault(u => u.UserId == userId);

        if (user == null)
            return Json(new { success = false });

        user.Role = model.Role;
        db.SaveChanges();

        return Json(new { success = true });
    }

    [HttpPost]
    public ActionResult UploadProduct(ProductUploadViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "Invalid form submission." });

        var category = db.Category.FirstOrDefault(c => c.CategoryId == model.CategoryId);
        if (category == null)
            return Json(new { success = false, message = "Invalid category selected." });

        string categoryName = category.Name.ToLower();
        bool isValid = true;
        string errorMessage = "";

        if (categoryName == "3d model")
        {
            if (model.Files == null || model.Files.Count() != 1 || !model.Files.First().FileName.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
            {
                isValid = false;
                errorMessage = "3D Model requires exactly one .glb file.";
            }
        }
        else if (categoryName == "e-book" || categoryName == "ebook")
        {
            if (model.Files == null || model.Files.Count() != 1 || !model.Files.First().FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                isValid = false;
                errorMessage = "E-Book requires exactly one .pdf file.";
            }
        }
        else if (categoryName == "presentation slide")
        {
            if (model.Files == null || model.Files.Count() != 1 || !model.Files.First().FileName.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase))
            {
                isValid = false;
                errorMessage = "Presentation Slide requires exactly one .pptx file.";
            }
        }
        else if (categoryName == "graphics template")
        {
            if (model.Files == null || !model.Files.Any(f => f.FileName.EndsWith(".ai", StringComparison.OrdinalIgnoreCase)))
            {
                isValid = false;
                errorMessage = "Graphics Template must include at least one .ai file.";
            }
        }

        if (!isValid)
        {
            return Json(new { success = false, message = errorMessage });
        }

        var product = new Product
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CategoryId = model.CategoryId,
            SellerId = model.SellerId,
            CreatedAt = DateTime.Now,
            IsActive = true,
            Status = "Pending",
            LikeCount = 0,
            UnlikeCount = 0,
            ViewCount = 0
        };

        db.Product.Add(product);
        db.SaveChanges();

        string folderName = category.Name;
        string basePath = Server.MapPath($"~/Assets/{folderName}/product_{product.ProductId}");

        string thumbnailFolder = Path.Combine(basePath, "thumbnails");
        string fileFolder = Path.Combine(basePath, "files");

        Directory.CreateDirectory(thumbnailFolder);
        Directory.CreateDirectory(fileFolder);

        if (model.Thumbnail != null && model.Thumbnail.ContentLength > 0)
        {
            var extension = Path.GetExtension(model.Thumbnail.FileName);
            var thumbnailFileName = product.Name + extension;
            var thumbnailPath = Path.Combine(thumbnailFolder, thumbnailFileName);
            model.Thumbnail.SaveAs(thumbnailPath);

            product.Thumbnail = thumbnailFileName;
            db.SaveChanges();
        }

        if (model.Files != null)
        {
            foreach (var file in model.Files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var safeFileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(fileFolder, safeFileName);
                    file.SaveAs(filePath);
                }
            }
        }

        return Json(new { success = true });
    }

    [HttpPost]
    public ActionResult DeleteProduct(int productId)
    {
        var product = db.Product.Find(productId);
        if (product != null)
        {
            var currentUserId = GetUserId();
            if (product.SellerId == currentUserId)
            {
                var categoryFolder = product.Category.Name ?? "3D Model";
                var folderPath = Server.MapPath($"~/Assets/{categoryFolder}/product_{product.ProductId}");
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);

                db.Product.Remove(product);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }
        return Json(new { success = false });
    }

    public JsonResult GetNotifications()
    {
        var userId = GetUserId();

        if (userId == 0)
        {
            return Json(new { success = false, message = "Not logged in." }, JsonRequestBehavior.AllowGet);
        }

        var notificationsQuery = db.Notification
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToList();

        var notifications = notificationsQuery.Select(n => new NotificationModel
        {
            NotificationId = n.NotificationId,
            Subject = n.Subject,
            Message = n.Message,
            Status = n.Status,
            Type = n.Type,
            CreatedAt = n.CreatedAt,
            UserId = n.UserId,
        }).ToList();

        var formattedNotifications = notifications.Select(n => new
        {
            n.NotificationId,
            n.Subject,
            n.Message,
            n.Status,
            n.Type,
            CreatedAt = n.CreatedAt.ToString("dd MMM yyyy")
        });

        return Json(new { success = true, notifications = formattedNotifications }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult DeleteNotification(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Json(new { success = false, message = "User session not found." });
        }

        var notification = db.Notification.FirstOrDefault(n => n.NotificationId == id && n.UserId == userId);

        if (notification == null)
        {
            return Json(new { success = false, message = "Notification not found or unauthorized." });
        }

        db.Notification.Remove(notification);
        db.SaveChanges();

        return Json(new { success = true });
    }

    int GetUserId()
    {
        if (Session["UserId"] != null)
        {
            return (int)Session["UserId"];
        }
        return 0; 
    }
    public class RoleChangeRequest
    {
        public string Role { get; set; }
    }
}
