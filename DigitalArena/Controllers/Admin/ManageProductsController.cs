using DigitalArena.DBContext;
using DigitalArena.Models.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;

namespace DigitalArena.Controllers.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ManageProductsController : Controller
    {

        private readonly DigitalArenaDBContext _dbContext = new DigitalArenaDBContext();


        [Route("admin/manage-products/product-details", Name = "ViewProductDetailsRoute")]
        public ActionResult ViewProductDetails(int id)
        {
            var product = _dbContext.Product
                .Include("Category")
                .Include("User")
                .Include("ProductFile")
                .Include("Review.User")
                .Include("Permission")
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Filter approved reviews
            var approvedReviews = product.Review
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            // Count how many people purchased it (valid permissions)
            int totalPurchases = product.Permission.Count(p => p.IsValid);

            var viewModel = new AdminProductDetailViewModel
            {
                Product = product,
                Category = product.Category,
                Seller = product.User,
                ProductFiles = product.ProductFile.ToList(),
                Reviews = approvedReviews,
                PurchaseCount = totalPurchases
            };

            return View(viewModel);
        }



        // Download Product Files
        public ActionResult DownloadSingleFile(int fileId)
        {
            var file = _dbContext.ProductFile.FirstOrDefault(f => f.ProductFileId == fileId);
            if (file == null)
                return HttpNotFound();

            // Adjust the path as per your actual folder structure
            var filePath = Server.MapPath("~/Assets/"+file.Product.Category.Name+"/product_"+file.ProductId+"/files/" + file.FileName);

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound();

            return File(filePath, "application/octet-stream", file.FileName);
        }


        public ActionResult DownloadAllFiles(int productId)
        {
            var files = _dbContext.ProductFile
                .Where(f => f.ProductId == productId)
                .ToList();

            if (!files.Any())
            {
                return HttpNotFound("No files found for this product.");
            }

            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        // File path format: ~/Assets/{ProductId}/files/{FileName}
                        string filePath = Server.MapPath($"~/Assets/{file.Product.Category.Name}/product_{file.ProductId}/files/{file.FileName}");

                        if (System.IO.File.Exists(filePath))
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                            var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);

                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(fileBytes, 0, fileBytes.Length);
                            }
                        }
                    }
                }

                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", $"Product_{productId}_Files.zip");
            }
        }


        [HttpPost]
        public ActionResult UpdateReviewStatus(int reviewId, string newStatus)
        {
            var review = _dbContext.Review.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return HttpNotFound();

            review.Status = newStatus;
            _dbContext.SaveChanges();

            return RedirectToAction("ViewProductDetails", new { id = review.ProductId });
        }


        [HttpPost]
        public ActionResult UpdateProductStatus(int productId, string newStatus)
        {
            var product = _dbContext.Product.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                return HttpNotFound();

            product.Status = newStatus;
            _dbContext.SaveChanges();

            return RedirectToAction("ViewProductDetails", new { id = productId });
        }


    }
}