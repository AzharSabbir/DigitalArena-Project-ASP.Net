using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalArena.Models;

namespace DigitalArena.Controllers
{
    public class ReviewController : Controller
    {
        private DigitalArenaDBContext db = new DigitalArenaDBContext();

        [HttpPost]
        public JsonResult AddReview(int productId, int rating, string comment)
        {
            int userId = GetCurrentUserId();
            try
            {
                // Create a new instance of the Review entity (not ReviewModel)
                var review = new Review
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now,
                    Status = "Pending"
                };
                // Add the Review entity to the database context
                db.Review.Add(review);
                db.SaveChanges();

                // Create a notification for the user
                var notification = new Notification
                {
                    Subject = "Review Submitted",
                    Message = "Your review has been submitted and is currently pending approval.",
                    CreatedAt = DateTime.Now,
                    Type = "Review",
                    Status = "Pending",
                    UserId = userId
                };

                db.Notification.Add(notification);
                db.SaveChanges();
                var timeAgo = DigitalArena.Helpers.DateHelper.GetPublishedAgo(review.CreatedAt);

                return Json(new
                {
                    success = true,
                    reviewId = review.ReviewId,
                    username = "You",
                    createdAt = timeAgo
                });
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, error = errorMessage });
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
            try
            {
                var review = db.Review.FirstOrDefault(r => r.ReviewId == reviewId);
                if (review == null)
                {
                    return Json(new { success = false, message = "Review not found." });
                }

                review.Comment = comment;
                review.Rating = rating;
                review.Status = "Approved"; 
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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