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

            // Build dictionaries for fast lookup
            var avgRatings = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(r => r.Rating)
                );

            var ratingCounts = reviews
                .GroupBy(r => r.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            var downloadCounts = permissions
                .GroupBy(p => p.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );

            // Get min and max values for normalization
            double minViews = products.Min(p => p.ViewCount);
            double maxViews = products.Max(p => p.ViewCount);
            double minDownloads = downloadCounts.Values.DefaultIfEmpty(0).Min();
            double maxDownloads = downloadCounts.Values.DefaultIfEmpty(0).Max();
            double minLikes = products.Min(p => p.LikeCount);
            double maxLikes = products.Max(p => p.LikeCount);
            double minUnlikes = products.Min(p => p.UnlikeCount);
            double maxUnlikes = products.Max(p => p.UnlikeCount);
            double minRating = avgRatings.Values.DefaultIfEmpty(0).Min();
            double maxRating = avgRatings.Values.DefaultIfEmpty(0).Max();
            double minRatingCount = ratingCounts.Values.DefaultIfEmpty(0).Min();
            double maxRatingCount = ratingCounts.Values.DefaultIfEmpty(0).Max();

            var now = DateTime.UtcNow;

            var trendingList = products.Select(p =>
            {
                int downloads = downloadCounts.ContainsKey(p.ProductId) ? downloadCounts[p.ProductId] : 0;
                double avgRating = avgRatings.ContainsKey(p.ProductId) ? avgRatings[p.ProductId] : 0;
                int ratingCount = ratingCounts.ContainsKey(p.ProductId) ? ratingCounts[p.ProductId] : 0;

                double age = (now - p.CreatedAt).TotalDays;
                double timeDecay = Math.Exp(-0.1 * age);

                double normViews = Normalize(p.ViewCount, minViews, maxViews);
                double normDownloads = Normalize(downloads, minDownloads, maxDownloads);
                double normLikes = Normalize(p.LikeCount, minLikes, maxLikes);
                double normUnlikes = Normalize(p.UnlikeCount, minUnlikes, maxUnlikes);
                double normRating = Normalize(avgRating, minRating, maxRating);
                double normRatingCount = Normalize(ratingCount, minRatingCount, maxRatingCount);

                double trendScore =
                    (normViews * 0.3) +
                    (normDownloads * 0.4) +
                    (normRating * 0.1) +
                    (normRatingCount * 0.05) +
                    (normLikes * 0.05) -
                    (normUnlikes * 0.05) +
                    (timeDecay * 0.1);

                return new
                {
                    p.ProductId,
                    p.Name,
                    p.Thumbnail,
                    p.CreatedAt,
                    p.LikeCount,
                    p.UnlikeCount,
                    DownloadCount = downloads,
                    ViewCount = p.ViewCount,
                    Ratings = new
                    {
                        Average = Math.Round(avgRating, 2),
                        Count = ratingCount
                    },
                    TrendScore = Math.Round(trendScore, 4)
                };
            })
            .OrderByDescending(p => p.TrendScore)
            .ToList();

            return Ok(trendingList);
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
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
