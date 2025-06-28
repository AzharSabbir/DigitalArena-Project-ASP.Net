using DigitalArena.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalArena.DBContext;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace DigitalArena.Controllers
{
    public class OrderController : Controller
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

        [HttpPost]
        public JsonResult FakePay(PaymentRequestModel request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var product = db.Product.Find(request.ProductId);
                if (product == null || product.Price != request.Amount)
                    return Json(new { success = false, error = "Invalid product or amount" });

                var order = new Order
                {
                    Amount = product.Price,
                    BillingAddress = "Dummy Address",
                    CreatedAt = DateTime.Now,
                    Status = "Completed",
                    PaymentStatus = "Paid",
                    UserId = userId
                };
                db.Order.Add(order);
                db.SaveChanges();

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Price = product.Price
                };
                db.OrderItem.Add(orderItem);
                db.SaveChanges();

                string pdfUrl = Url.Action("DownloadInvoice", "Order", new { orderId = order.OrderId });
                return Json(new { success = true, pdfUrl = pdfUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult DownloadInvoice(int orderId)
        {
            int userId = GetCurrentUserId();

            // Load the order with products
            var orderEntity = db.Order
                                .Include("OrderItem.Product")
                                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == userId);

            if (orderEntity == null)
                return HttpNotFound("Order not found or access denied.");

            // Map EF entity to ViewModel
            var order = new OrderModel
            {
                OrderId = orderEntity.OrderId,
                Amount = orderEntity.Amount,
                BillingAddress = orderEntity.BillingAddress,
                CreatedAt = orderEntity.CreatedAt,
                Status = orderEntity.Status,
                PaymentStatus = orderEntity.PaymentStatus,
                UserId = orderEntity.UserId,
                OrderItems = orderEntity.OrderItem.Select(oi => new OrderItemModel
                {
                    OrderItemId = oi.OrderItemId,
                    Price = oi.Price,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    Product = new ProductModel
                    {
                        ProductId = oi.Product.ProductId,
                        Name = oi.Product.Name,
                        Price = oi.Product.Price,
                        Thumbnail = oi.Product.Thumbnail
                    }
                }).ToList()
            };

            // Generate PDF
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                doc.Add(new Paragraph("INVOICE", titleFont));
                doc.Add(new Paragraph(" "));

                // Order Summary
                doc.Add(new Paragraph($"Order ID: {order.OrderId}", normalFont));
                doc.Add(new Paragraph($"Order Date: {order.CreatedAt.ToString("dd MMM yyyy")}", normalFont));
                doc.Add(new Paragraph($"Status: {order.Status}", normalFont));
                doc.Add(new Paragraph($"Payment: {order.PaymentStatus}", normalFont));
                doc.Add(new Paragraph($"Billing Address: {order.BillingAddress ?? "N/A"}", normalFont));
                doc.Add(new Paragraph(" "));

                // Table: Products
                var table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3, 1 });

                table.AddCell(new PdfPCell(new Phrase("Product Name", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Price", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var item in order.OrderItems)
                {
                    table.AddCell(new Phrase(item.Product?.Name ?? "N/A", normalFont));
                    table.AddCell(new Phrase("$" + item.Price.ToString("F2"), normalFont));
                }

                doc.Add(table);

                // Total
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Total Amount: ${order.Amount:F2}", titleFont));

                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"Invoice_Order_{order.OrderId}.pdf");
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