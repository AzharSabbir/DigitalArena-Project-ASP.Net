using DigitalArena.DBContext;
using DigitalArena.Models;
using DigitalArena.Helpers;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    public class ProductDetailsController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();

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

            // Purchase Count
            int downloadCount = db.Permission.Count(p => p.ProductId == id && p.IsValid);

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


            // 🎯 Suggested Products (Same category, excluding current product)
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



            // Fix for CS0029: Update the type of SuggestedProducts in the ProductDetailsViewModel initialization
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
                    LikeCount = productEntity.LikeCount,
                    UnlikeCount = productEntity.UnlikeCount,
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
                DownloadCount = downloadCount,
                ModelPath = modelPath,
                SuggestedProducts = suggestedProducts // Change this to use the original list of Product entities
            };

            return View(viewModel);
        }

    }

}
