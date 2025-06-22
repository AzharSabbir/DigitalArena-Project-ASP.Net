using DigitalArena.DBContext;
using DigitalArena.Models;
using DigitalArena.Helpers;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

namespace DigitalArena.Controllers
{
    public class ProductDetailsController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();

        // GET: ProductDetails/Details/5
        // GET: ProductDetails/Details/5
        public ActionResult ProductDetails(int id)
        {
            var productEntity = db.Product
                                  .Include(p => p.Category)
                                  .FirstOrDefault(p => p.ProductId == id && p.IsActive);

            if (productEntity == null)
            {
                return HttpNotFound();
            }

            // Increase View Count
            productEntity.ViewCount++;
            db.SaveChanges();

            // Get the current user ID
            int userId = GetCurrentUserId(); // Replace with actual user authentication logic

            // Check if the user has liked or disliked the product
            var engagement = db.Engagement
                                .FirstOrDefault(e => e.ProductId == id && e.UserId == userId);

            bool isLiked = false;
            bool isDisliked = false;

            if (engagement != null)
            {
                isLiked = engagement.Liked;
                isDisliked = engagement.Disliked;
            }

            // Get like and dislike counts directly from the Product table
            int likeCount = productEntity.LikeCount;  // Directly from Product table
            int dislikeCount = productEntity.UnlikeCount;  // Directly from Product table

            // Check if the product is in the user's cart
            var isInCart = db.CartItem
                             .Include(ci => ci.Cart)
                             .Any(ci => ci.ProductId == id && ci.Cart.UserId == userId);

            // Reviews for the product
            var reviews = db.Review
                            .Include(r => r.User)
                            .Where(r => r.ProductId == id && r.Status == "Approved")
                            .ToList();

            // Published Ago
            string publishedAgo = DateHelper.GetPublishedAgo(productEntity.CreatedAt);

            // Model File Path
            string modelFile = Path.GetFileNameWithoutExtension(productEntity.Thumbnail) + ".glb";
            string browseCategory = productEntity.Category?.Name?.Replace(" ", "%20");
            string modelPath = Url.Content($"~/Assets/{browseCategory}/product_{productEntity.ProductId}/files/{modelFile}");

            // Suggested Products (Same category, excluding current product)
            var suggestedEntities = db.Product
                                      .Where(p => p.IsActive &&
                                                  p.ProductId != id &&
                                                  p.CategoryId == productEntity.CategoryId)
                                      .OrderByDescending(p => p.ViewCount)
                                      .Take(4)
                                      .ToList();

            var allSuggested = db.Product
                                 .Where(p => p.IsActive &&
                                             p.Status == "Approved" && // ✅ Only approved products
                                             p.ProductId != id &&
                                             p.CategoryId == productEntity.CategoryId)
                                 .ToList();

            var random = new Random();
            var shuffledSuggested = allSuggested
                .OrderBy(p => random.Next())
                .Take(12)
                .ToList();

            var suggestedProducts = shuffledSuggested.Select(p => new ProductModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                Thumbnail = p.Thumbnail,
                CategoryId = p.CategoryId,
                LikeCount = p.LikeCount,
                ViewCount = p.ViewCount
            }).ToList();

            var viewModel = new ProductDetailsViewModel
            {
                Product = new ProductModel
                {
                    ProductId = productEntity.ProductId,
                    Name = productEntity.Name,
                    Description = productEntity.Description,
                    Price = productEntity.Price,
                    Thumbnail = productEntity.Thumbnail,
                    IsActive = productEntity.IsActive,
                    CreatedAt = productEntity.CreatedAt,
                    LikeCount = likeCount,  // Directly from Product table
                    UnlikeCount = dislikeCount,  // Directly from Product table
                    Status = productEntity.Status,
                    CategoryId = productEntity.CategoryId,
                    SellerId = productEntity.SellerId,
                    ViewCount = productEntity.ViewCount,
                    Category = productEntity.Category != null ? new CategoryModel
                    {
                        CategoryId = productEntity.Category.CategoryId,
                        Name = productEntity.Category.Name,
                        Description = productEntity.Category.Description,
                        IsActive = productEntity.Category.IsActive,
                        CategoryImage = productEntity.Category.CategoryImage
                    } : null
                },
                Reviews = reviews.Select(r => new ReviewModel
                {
                    ReviewId = r.ReviewId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status,
                    UserId = r.UserId,
                    ProductId = r.ProductId,
                    User = new UserModel
                    {
                        UserId = r.User.UserId,
                        Username = r.User.Username
                    }
                }).ToList(),
                PublishedAgo = publishedAgo,
                DownloadCount = productEntity.Permission.Count(p => p.ProductId == id && p.IsValid),
                ModelPath = modelPath,
                SuggestedProducts = suggestedProducts,
                IsLiked = isLiked,
                IsDisliked = isDisliked,
                IsInCart = isInCart // Add IsInCart field to ViewModel
            };

            return View(viewModel);
        }


        // Toggle Add to Cart (Add/Remove from Cart)
        [HttpPost]
        public JsonResult ToggleCart(int productId)
        {
            int userId = GetCurrentUserId();
            var cartItem = db.CartItem
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.ProductId == productId && ci.Cart.UserId == userId);

            bool inCart;
            if (cartItem != null)
            {
                db.CartItem.Remove(cartItem); // Remove from cart
                inCart = false;
            }
            else
            {
                var cart = db.Cart.FirstOrDefault(c => c.UserId == userId)
                           ?? new Cart { UserId = userId };
                if (cart.CartId == 0) { db.Cart.Add(cart); db.SaveChanges(); }

                db.CartItem.Add(new CartItem
                {
                    ProductId = productId,
                    CartId = cart.CartId,
                    CreatedAt = DateTime.Now
                });
                inCart = true;
            }

            db.SaveChanges();
            return Json(new { success = true, inCart });
        }

        // Toggle Like (Add/Remove Like)
        [HttpPost]
        public JsonResult ToggleLike(int productId)
        {
            int userId = 5; // Get the current user's ID (from session or authentication)

            // Check if the product already exists in the engagement table
            var engagement = db.Engagement
                               .FirstOrDefault(e => e.ProductId == productId && e.UserId == userId);

            if (engagement == null)
            {
                engagement = new Engagement
                {
                    ProductId = productId,
                    UserId = userId,
                    Liked = true,
                    Disliked = false
                };
                db.Engagement.Add(engagement);
                db.SaveChanges();

                // Increment the like count in the Product table
                var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    product.LikeCount++;  // Increase Like count
                    db.SaveChanges();
                }
            }
            else
            {
                // Toggle like status
                if (engagement.Liked)
                {
                    // If it's already liked, we will remove the like and decrease the count
                    engagement.Liked = false;

                    // Decrease like count in the Product table
                    var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        product.LikeCount--;  // Decrease Like count
                        db.SaveChanges();
                    }
                }
                else
                {
                    // If it's not liked, add the like and increase the count
                    engagement.Liked = true;
                    engagement.Disliked = false; // Dislike is removed when liking the product

                    // Increase like count in the Product table
                    var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        product.LikeCount++;  // Increase Like count
                        db.SaveChanges();
                    }
                }
            }

            db.SaveChanges();

            // Get updated like count
            int newLikeCount = db.Product.FirstOrDefault(p => p.ProductId == productId)?.LikeCount ?? 0;

            return Json(new { success = true, liked = engagement.Liked, newLikeCount });
        }

        // Toggle Dislike (Add/Remove Dislike)
        [HttpPost]
        public JsonResult ToggleDislike(int productId)
        {
            int userId = 5; // Get the current user's ID (from session or authentication)

            // Check if the product already exists in the engagement table
            var engagement = db.Engagement
                               .FirstOrDefault(e => e.ProductId == productId && e.UserId == userId);

            if (engagement == null)
            {
                engagement = new Engagement
                {
                    ProductId = productId,
                    UserId = userId,
                    Liked = false,
                    Disliked = true
                };
                db.Engagement.Add(engagement);
                db.SaveChanges();

                // Increment the dislike count in the Product table
                var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                if (product != null)
                {
                    product.UnlikeCount++;  // Increase Dislike count
                    db.SaveChanges();
                }
            }
            else
            {
                // Toggle dislike status
                if (engagement.Disliked)
                {
                    // If it's already disliked, we will remove the dislike and decrease the count
                    engagement.Disliked = false;

                    // Decrease dislike count in the Product table
                    var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        product.UnlikeCount--;  // Decrease Dislike count
                        db.SaveChanges();
                    }
                }
                else
                {
                    // If it's not disliked, add the dislike and increase the count
                    engagement.Disliked = true;
                    engagement.Liked = false; // Like is removed when disliking the product

                    // Increase dislike count in the Product table
                    var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
                    if (product != null)
                    {
                        product.UnlikeCount++;  // Increase Dislike count
                        db.SaveChanges();
                    }
                }
            }

            db.SaveChanges();

            // Get updated dislike count
            int newDislikeCount = db.Product.FirstOrDefault(p => p.ProductId == productId)?.UnlikeCount ?? 0;

            return Json(new { success = true, disliked = engagement.Disliked, newDislikeCount });
        }


        private int GetCurrentUserId()
        {
            // Replace this with your actual user authentication/session logic
            return 5; // Example hardcoded User ID for testing
        }


    }
}
