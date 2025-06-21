using DigitalArena.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    public class TrendingProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public TrendingProductsController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:64977/")
            };
        }

        public async Task<ActionResult> Trending()
        {
            var response = await _httpClient.GetAsync("api/trending-products");

            if (!response.IsSuccessStatusCode)
                return View(new List<ProductModel>());

            var json = await response.Content.ReadAsStringAsync();
            var apiData = JsonConvert.DeserializeObject<List<dynamic>>(json);

            // Extract IDs
            var productIds = apiData.Select(p => (int)p.ProductId).ToList();

            // Query database for product details using EF
            using (var db = new DigitalArena.DBContext.DigitalArenaDBContext())
            {
                var products = db.Product
                    .Include("Category")
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToList();

                var avgRatings = db.Review
                    .Where(r => productIds.Contains(r.ProductId) && r.Status == "Approved")
                    .GroupBy(r => r.ProductId)
                    .ToDictionary(g => g.Key, g => g.Average(r => r.Rating));

                var downloadCounts = db.Permission
                    .Where(p => p.IsValid && productIds.Contains(p.ProductId))
                    .GroupBy(p => p.ProductId)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Build model
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

                ViewBag.AvgRatings = avgRatings;
                ViewBag.DownloadCounts = downloadCounts;
                ViewBag.BrowseCategory = "Trending";

                return View("Trending", model);
            }
        }
    }
}
