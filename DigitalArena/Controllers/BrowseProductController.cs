using DigitalArena.DBContext;
using DigitalArena.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    [AllowAnonymous]
    public class BrowseProductController : Controller
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();
        public async Task<ActionResult> BrowseCategory(string name, int page = 1, int pageSize = 9)
        {
            if (string.IsNullOrWhiteSpace(name))
                return HttpNotFound("Category name is required.");

            var trimmedName = name.Trim();

            var category = db.Category
                .AsNoTracking()
                .FirstOrDefault(c => c.IsActive &&
                                     !string.IsNullOrEmpty(c.Name) &&
                                     c.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
                return HttpNotFound("Category not found.");

            page = Math.Max(page, 1);

            var query = db.Product
                .Include(p => p.Category)
                .Where(p => p.IsActive &&
                            p.Status == "Approved" &&
                            p.CategoryId == category.CategoryId);

            var totalProducts = query.Count();

            var products = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var productIds = products.Select(p => p.ProductId).ToList();

            var avgRatings = db.Review
                .Where(r => productIds.Contains(r.ProductId) && r.Status == "Approved")
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(r => r.Rating), 2)
                );

            var downloadCounts = db.Permission
                .Where(p => p.IsValid && productIds.Contains(p.ProductId))
                .GroupBy(p => p.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            Dictionary<int, double> trendingScores = new Dictionary<int, double>();
            try
            {
                string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
                string apiUrl = $"{baseUrl}/api/trending-products";

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        var trendingData = JsonConvert.DeserializeObject<List<TrendingProductViewModel>>(jsonData);

                        trendingScores = trendingData.ToDictionary(p => p.ProductId, p => p.TrendScore);
                    }
                }
            }
            catch (Exception ex)
            {
                trendingScores = new Dictionary<int, double>();
            }

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
                },
                TrendScore = trendingScores[p.ProductId]
            }).ToList();

            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                var user = db.User.FirstOrDefault(u => u.Email == userName);
                userId = Session["UserId"] as int? ?? user?.UserId;
            }

            List<int> purchasedIds = new List<int>();
            if (userId.HasValue)
            {
                purchasedIds = db.Permission
                    .Where(p => p.IsValid && p.UserId == userId.Value)
                    .Select(p => p.ProductId)
                    .Distinct()
                    .ToList();
            }

            List<int> reviewedIds = new List<int>();
            if (userId.HasValue)
            {
                reviewedIds = db.Review
                    .Where(r => r.UserId == userId.Value)
                    .Select(r => r.ProductId)
                    .Distinct()
                    .ToList();
            }

            ViewBag.BrowseCategory = category.Name;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.AvgRatings = avgRatings;
            ViewBag.DownloadCounts = downloadCounts;
            ViewBag.PurchasedIds = purchasedIds;
            ViewBag.ReviewedIds = reviewedIds;

            return View("BrowseCategory", model);
        }

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
