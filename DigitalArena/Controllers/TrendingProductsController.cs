using DigitalArena.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DigitalArena.Controllers
{
    public class TrendingProductsController : Controller
    {
        public async Task<ActionResult> Trending()
        {
            // ✅ Build the full API URL using the current request context
            var baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
            var apiUrl = $"{baseUrl}/api/trending-products";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();

                        // ⛳ Replace with your actual view model if different
                        var trendingData = JsonConvert.DeserializeObject<List<TrendingProductViewModel>>(jsonData);

                        return View(trendingData);
                    }

                    ViewBag.Error = "Could not load trending products.";
                    return View(new List<TrendingProductViewModel>());
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Something went wrong: " + ex.Message;
                    return View(new List<TrendingProductViewModel>());
                }
            }
        }
    }
}
