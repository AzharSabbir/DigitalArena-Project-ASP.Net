using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using DigitalArena.Models;



namespace DigitalArena.Controllers
{
    public class ProductDetailsController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();

        //GET: ProductDetails/Details/5
        public ActionResult ProductDetails(int id)
        {
            var productEntity = db.Product
                                  .Include(p => p.Category)
                                  .FirstOrDefault(p => p.ProductId == id && p.IsActive);

            if (productEntity == null)
            {
                return HttpNotFound();
            }

            // <---------- Purchase Count ---------->
            var downloadCount = db.Permission.Count(p => p.ProductId == id && p.IsValid);
            ViewBag.DownloadCount = downloadCount;

            // <---------- Published Ago ---------->
            var createdAt = productEntity.CreatedAt;
            var timeSpan = DateTime.Now - createdAt;

            string publishedAgo;

            if (timeSpan.TotalDays >= 365)
            {
                int years = (int)(timeSpan.TotalDays / 365);
                publishedAgo = $"{years} year{(years > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalDays >= 30)
            {
                int months = (int)(timeSpan.TotalDays / 30);
                publishedAgo = $"{months} month{(months > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalDays >= 1)
            {
                int days = (int)timeSpan.TotalDays;
                publishedAgo = $"{days} day{(days > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                publishedAgo = $"{hours} hour{(hours > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                publishedAgo = $"{minutes} minute{(minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                int seconds = (int)timeSpan.TotalSeconds;
                publishedAgo = $"{seconds} second{(seconds > 1 ? "s" : "")} ago";
            }

            ViewBag.PublishedAgo = publishedAgo;


            var productModel = new ProductModel
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
                Category = productEntity.Category != null ? new CategoryModel
                {
                    CategoryId = productEntity.Category.CategoryId,
                    Name = productEntity.Category.Name,
                    Description = productEntity.Category.Description,
                    IsActive = productEntity.Category.IsActive,
                    CategoryImage = productEntity.Category.CategoryImage
                } : null
            };

            ViewBag.ModelFile = Path.GetFileNameWithoutExtension(productModel.Thumbnail) + ".glb";
            var browseCategory = productModel.Category?.Name?.Replace(" ", "%20"); // optional URL-encoding
            ViewBag.ModelPath = Url.Content($"~/Assets/{browseCategory}/product_{productModel.ProductId}/files/{ViewBag.ModelFile}");

            return View(productModel);
        }
    } 
}