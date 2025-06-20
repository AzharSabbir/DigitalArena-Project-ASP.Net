using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    public class ReviewController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult AddReview(int productId, int rating, string comment)
        {
            try
            {
                // Create a new instance of the Review entity (not ReviewModel)
                var review = new Review
                {
                    ProductId = productId,
                    UserId = 5,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    Status = "Approved"
                };

                // Add the Review entity to the database context
                db.Review.Add(review);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    reviewId = review.ReviewId,
                    username = "You",
                    createdAt = review.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteReview(int reviewId)
        {
            var review = db.Review.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }

            db.Review.Remove(review);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UpdateReview(int reviewId, string comment, int rating)
        {
            var review = db.Review.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }

            review.Comment = comment;
            review.Rating = rating;
            review.Status = "Pending"; // Re-moderate after edit
            review.CreatedAt = DateTime.Now;

            db.SaveChanges();

            return Json(new { success = true, updatedAt = review.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") });
        }


    }
}