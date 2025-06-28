using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalArena.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ManageReviewController : Controller
    {

        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();


        [HttpPost]
        public ActionResult ChangeReviewStatus(int reviewId, string newStatus)
        {
            var review = _dbContext.Review.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                review.Status = newStatus;
                _dbContext.SaveChanges();
                TempData["Success"] = "Review status updated successfully.";
            }
            return RedirectToRoute("ManageReviewsRoute");
        }

    }
}