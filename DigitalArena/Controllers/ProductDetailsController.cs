using DigitalArena.DBContext;
using DigitalArena.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.IO;
using DigitalArena.Models;
using DigitalArena.Helpers;



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

            // <---------- Increase View Count ---------->
            productEntity.ViewCount++;

            // <---------- Purchase Count ---------->
            var downloadCount = db.Permission.Count(p => p.ProductId == id && p.IsValid);
            ViewBag.DownloadCount = downloadCount;

            // <---------- Published Ago ---------->
            ViewBag.PublishedAgo = DateHelper.GetPublishedAgo(productEntity.CreatedAt);


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
                ViewCount = productEntity.ViewCount,
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
            var browseCategory = productModel.Category?.Name?.Replace(" ", "%20");
            ViewBag.ModelPath = Url.Content($"~/Assets/{browseCategory}/product_{productModel.ProductId}/files/{ViewBag.ModelFile}");

            return View(productModel);
        }
    } 
}