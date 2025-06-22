using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DigitalArena.DBContext;
using DigitalArena.Models;

public class CartController : Controller
{
    private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

    // GET: Cart/Cart
    public ActionResult Cart()
    {
        int userId = GetCurrentUserId();

        var cart = db.Cart.Include(c => c.User)
                          .FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            return View(new List<CartItemModel>());
        }

        var cartItems = db.CartItem
            .Where(ci => ci.CartId == cart.CartId)
            .Include(ci => ci.Product)
            .Include(ci => ci.Product.Category)
            .Include(ci => ci.Product.User)
            .ToList();

        var cartModels = cartItems.Select(ci => new CartItemModel
        {
            CartItemId = ci.CartItemId,
            CreatedAt = ci.CreatedAt,
            CartId = ci.CartId,
            ProductId = ci.ProductId,
            Product = new ProductModel
            {
                ProductId = ci.Product.ProductId,
                Name = ci.Product.Name,
                Description = ci.Product.Description,
                Price = ci.Product.Price,
                Thumbnail = ci.Product.Thumbnail,
                ViewCount = ci.Product.ViewCount,
                LikeCount = ci.Product.LikeCount,
                Category = new CategoryModel
                {
                    CategoryId = ci.Product.Category.CategoryId,
                    Name = ci.Product.Category.Name
                },
                User = new UserModel
                {
                    UserId = ci.Product.User.UserId,
                    FullName = ci.Product.User.FullName,
                    Username = ci.Product.User.Username,
                    Email = ci.Product.User.Email,
                    Role = ci.Product.User.Role,
                    ProfileImage = ci.Product.User.ProfileImage
                }
            }
        }).ToList();

        return View(cartModels);
    }

    // POST: Cart/Add
    [HttpPost]
    public JsonResult Toggle(int productId)
    {
        int userId = GetCurrentUserId();

        var cart = db.Cart.FirstOrDefault(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            db.Cart.Add(cart);
            db.SaveChanges();
        }

        var existingItem = db.CartItem
            .FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

        if (existingItem != null)
        {
            db.CartItem.Remove(existingItem);
            db.SaveChanges();
            return Json(new { success = true, added = false });
        }
        else
        {
            db.CartItem.Add(new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                CreatedAt = DateTime.Now
            });
            db.SaveChanges();
            return Json(new { success = true, added = true });
        }
    }

    // POST: Cart/Remove
    [HttpPost]
    public ActionResult Remove(int cartItemId)
    {
        var item = db.CartItem.Find(cartItemId);

        if (item != null)
        {
            db.CartItem.Remove(item);
            db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // GET: Cart/Checkout
    public ActionResult Checkout()
    {
        int userId = GetCurrentUserId();
        // Logic to collect cart items and show summary for checkout
        // Possibly redirect to a payment gateway or summary page
        return RedirectToAction("Index", "Checkout");
    }

    private int GetCurrentUserId()
    {
        // Replace this with your actual user session logic
        return 5;
    }
}
