using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using DigitalArena.DBContext;
using DigitalArena.Models;

namespace DigitalArena.Controllers.API
{
    [RoutePrefix("api/trending-products")]
    public class TrendingProductsApiController : ApiController
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetTrendingProducts()
        {
            var products = db.Product
                .Where(p => p.IsActive && p.Status == "Approved")
                .Include(p => p.Category)
                .ToList();

            if (!products.Any())
                return Ok(new List<object>());

            var productIds = products.Select(p => p.ProductId).ToList();

            var reviews = db.Review
                .Where(r => r.Status == "Approved" && productIds.Contains(r.ProductId))
                .ToList();

            var permissions = db.Permission
                .Where(p => p.IsValid && productIds.Contains(p.ProductId))
                .ToList();

            var ratingsList = reviews
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    AvgRating = g.Average(r => r.Rating),
                    RatingCount = g.Count()
                }).ToList();

            var downloadList = permissions
                .GroupBy(p => p.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    DownloadCount = g.Count()
                }).ToList();

            double minViews = products.Min(p => (double)p.ViewCount);
            double maxViews = products.Max(p => (double)p.ViewCount);
            double minLikes = products.Min(p => (double)p.LikeCount);
            double maxLikes = products.Max(p => (double)p.LikeCount);
            double minUnlikes = products.Min(p => (double)p.UnlikeCount);
            double maxUnlikes = products.Max(p => (double)p.UnlikeCount);
            double minDownloads = downloadList.Any() ? downloadList.Min(d => (double)d.DownloadCount) : 0;
            double maxDownloads = downloadList.Any() ? downloadList.Max(d => (double)d.DownloadCount) : 1;
            double minRating = ratingsList.Any() ? ratingsList.Min(r => r.AvgRating) : 0;
            double maxRating = ratingsList.Any() ? ratingsList.Max(r => r.AvgRating) : 1;
            double minRatingCount = ratingsList.Any() ? ratingsList.Min(r => (double)r.RatingCount) : 0;
            double maxRatingCount = ratingsList.Any() ? ratingsList.Max(r => (double)r.RatingCount) : 1;

            var now = DateTime.UtcNow;

            var trending = new List<object>();

            foreach (var product in products)
            {
                var ratingInfo = ratingsList.FirstOrDefault(r => r.ProductId == product.ProductId);
                var downloadInfo = downloadList.FirstOrDefault(d => d.ProductId == product.ProductId);

                double avgRating = ratingInfo?.AvgRating ?? 0;
                int ratingCount = ratingInfo?.RatingCount ?? 0;
                int downloadCount = downloadInfo?.DownloadCount ?? 0;

                double ageInDays = (now - product.CreatedAt).TotalDays;
                double timeDecay = Math.Exp(-0.1 * ageInDays);

                double trendScore =
                    (Normalize(product.ViewCount, minViews, maxViews) * 0.3) +
                    (Normalize(downloadCount, minDownloads, maxDownloads) * 0.4) +
                    (Normalize(avgRating, minRating, maxRating) * 0.1) +
                    (Normalize(ratingCount, minRatingCount, maxRatingCount) * 0.05) +
                    (Normalize(product.LikeCount, minLikes, maxLikes) * 0.05) -
                    (Normalize(product.UnlikeCount, minUnlikes, maxUnlikes) * 0.05) +
                    (timeDecay * 0.2);

                trending.Add(new
                {
                    product.ProductId,
                    product.Name,
                    product.Thumbnail,
                    product.CreatedAt,
                    product.LikeCount,
                    product.UnlikeCount,
                    ViewCount = product.ViewCount,
                    Price = product.Price,
                    CategoryName = product.Category.Name,
                    DownloadCount = downloadCount,
                    Ratings = new
                    {
                        Average = Math.Round(avgRating, 2),
                        Count = ratingCount
                    },
                    TrendScore = Math.Round(trendScore, 4)
                });
            }

            var sortedTrending = trending
                .OrderByDescending(t => ((dynamic)t).TrendScore)
                .ToList();

            return Ok(sortedTrending);
        }

        private double Normalize(double value, double min, double max)
        {
            if (Math.Abs(max - min) < 0.0001)
                return 0;
            return (value - min) / (max - min);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
