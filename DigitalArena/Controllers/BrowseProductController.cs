using DigitalArena.DBContext;
using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    public class BrowseProductController : Controller
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

        // <---------- Product Index ---------->
        public ActionResult Index()
        {
            return View();
        }

        // <---------- Browse by Category ---------->
        public ActionResult BrowseCategory(string name, int page = 1, int pageSize = 9)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return HttpNotFound("Category name is required.");
            }

            var trimmedName = name.Trim();

            var category = db.Category
                .AsNoTracking()
                .FirstOrDefault(c => c.IsActive &&
                                     !string.IsNullOrEmpty(c.Name) &&
                                     c.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return HttpNotFound("Category not found.");
            }

            page = page < 1 ? 1 : page;
            ViewBag.BrowseCategory = category.Name;

            var query = db.Product
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                            p.Status == "Approved" &&
                            p.CategoryId == category.CategoryId);

            var totalProducts = query.Count();

            var products = query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var productIds = products.Select(p => p.ProductId).ToList();

            var avgRatings = db.Review
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(r => r.Rating)
                );

            var downloadCounts = db.Permission
                .Where(p => p.IsValid && productIds.Contains(p.ProductId))
                .GroupBy(p => p.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            ViewBag.AvgRatings = avgRatings;
            ViewBag.DownloadCounts = downloadCounts;

            var model = products.Select(p => new ProductModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Thumbnail = p.Thumbnail,
                LikeCount = p.LikeCount,
                UnlikeCount = p.UnlikeCount,
                ViewCount = p.ViewCount, 
                CreatedAt = p.CreatedAt,
                Status = p.Status,
                CategoryId = p.CategoryId,
                SellerId = p.SellerId,
                Category = new CategoryModel
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    IsActive = p.Category.IsActive,
                    CategoryImage = p.Category.CategoryImage
                }
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            return View("BrowseCategory", model);
        }

        // <---------- Dispose DB Context ---------->
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
