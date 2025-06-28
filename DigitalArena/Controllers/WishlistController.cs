using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DigitalArena.DBContext;
using DigitalArena.Models; // Update this to match your actual namespace

public class WishlistController : Controller
{
    private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

    // GET: Wishlist/Index      
    public ActionResult Index()
    {
        int userId = GetCurrentUserId();

        var wishlistItems = db.Wishlist
            .Include(w => w.Product)
            .Include(w => w.Product.Category)
            .Include(w => w.Product.User)
            .Where(w => w.UserId == userId)
            .ToList();

        // Manually map to WishlistModel
        var wishlistModels = wishlistItems.Select(w => new WishlistModel
        {
            WishlistId = w.WishlistId,
            CreatedAt = w.CreatedAt,
            UserId = w.UserId,
            ProductId = w.ProductId,
            Product = new ProductModel
            {
                ProductId = w.Product.ProductId,
                Name = w.Product.Name,
                Description = w.Product.Description,
                Price = w.Product.Price,
                Thumbnail = w.Product.Thumbnail,
                IsActive = w.Product.IsActive,
                CreatedAt = w.Product.CreatedAt,
                LikeCount = w.Product.LikeCount,
                UnlikeCount = w.Product.UnlikeCount,
                Status = w.Product.Status,
                CategoryId = w.Product.CategoryId,
                SellerId = w.Product.SellerId,
                ViewCount = w.Product.ViewCount,

                Category = new CategoryModel
                {
                    CategoryId = w.Product.Category.CategoryId,
                    Name = w.Product.Category.Name
                },

                User = new UserModel
                {
                    UserId = w.Product.User.UserId,
                    FullName = w.Product.User.FullName,
                    Username = w.Product.User.Username,
                    Email = w.Product.User.Email,
                    Phone = w.Product.User.Phone,
                    Role = w.Product.User.Role,
                    ProfileImage = w.Product.User.ProfileImage
                }
            }
        }).ToList();

        return View(wishlistModels);
    }


    // POST: Wishlist/Add
    [HttpPost]
    public JsonResult Toggle(int productId)
    {
        int userId = GetCurrentUserId();

        var existing = db.Wishlist
            .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

        if (existing != null)
        {
            db.Wishlist.Remove(existing);
            db.SaveChanges();
            return Json(new { success = true, added = false });
        }
        else
        {
            db.Wishlist.Add(new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
            return Json(new { success = true, added = true });
        }
    }


    // POST: Wishlist/Remove
    [HttpPost]
    public ActionResult Remove(int wishlistId)
    {
        var item = db.Wishlist.Find(wishlistId);

        if (item != null)
        {
            db.Wishlist.Remove(item);
            db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    private int GetCurrentUserId()
    {
        // Replace this with your actual user authentication/session logic
        //return Convert.ToInt32(Session["UserId"]);
        return 5; // Example user ID for testing
    }
}
