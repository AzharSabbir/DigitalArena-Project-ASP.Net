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
    [AllowAnonymous]
    public class ProductDetailsController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();
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

            if (User.Identity.IsAuthenticated && engagement != null)
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

        [HttpPost]
        public JsonResult ToggleLike(int productId)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            var engagement = db.Engagement
                               .FirstOrDefault(e => e.ProductId == productId && e.UserId == userId);

            var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            bool wasDisliked = false;

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
                product.LikeCount++;
            }
            else
            {
                if (engagement.Liked)
                {
                    engagement.Liked = false;
                    product.LikeCount = Math.Max(0, product.LikeCount - 1);
                }
                else
                {
                    engagement.Liked = true;
                    product.LikeCount++;

                    if (engagement.Disliked)
                    {
                        engagement.Disliked = false;
                        product.UnlikeCount = Math.Max(0, product.UnlikeCount - 1);
                        wasDisliked = true;
                    }
                }
            }

            db.SaveChanges();

            return Json(new
            {
                success = true,
                liked = engagement.Liked,
                disliked = engagement.Disliked,
                newLikeCount = product.LikeCount,
                newDislikeCount = product.UnlikeCount
            });
        }
        [HttpPost]
        public JsonResult ToggleDislike(int productId)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false, message = "Unauthorized" });

            var engagement = db.Engagement
                               .FirstOrDefault(e => e.ProductId == productId && e.UserId == userId);

            var product = db.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            bool wasLiked = false;

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
                product.UnlikeCount++;
            }
            else
            {
                if (engagement.Disliked)
                {
                    engagement.Disliked = false;
                    product.UnlikeCount = Math.Max(0, product.UnlikeCount - 1);
                }
                else
                {
                    engagement.Disliked = true;
                    product.UnlikeCount++;

                    if (engagement.Liked)
                    {
                        engagement.Liked = false;
                        product.LikeCount = Math.Max(0, product.LikeCount - 1);
                        wasLiked = true;
                    }
                }
            }

            db.SaveChanges();

            return Json(new
            {
                success = true,
                liked = engagement.Liked,
                disliked = engagement.Disliked,
                newLikeCount = product.LikeCount,
                newDislikeCount = product.UnlikeCount
            });
        }


        int GetCurrentUserId()
        {
            if (Session["UserId"] != null)
            {
                return (int)Session["UserId"];
            }
            return 0;
        }

    }
}
