using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DigitalArena.Models;
using Newtonsoft.Json;

namespace DigitalArena.Services
{
    public class TypesenseService
    {
        private readonly HttpClient _httpClient;
        private const string API_KEY = "xyz123"; // 🔐 Set same as docker-compose
        private const string BASE_URL = "http://localhost:8200";

        public TypesenseService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL)
            };
            _httpClient.DefaultRequestHeaders.Add("X-TYPESENSE-API-KEY", API_KEY);
        }

        public async Task CreateSchemaIfNotExistsAsync()
        {
            var schema = new
            {
                name = "products",
                fields = new[]
                {
                    new { name = "product_id", type = "int32" },
                    new { name = "name", type = "string" },
                    new { name = "description", type = "string" },
                    new { name = "price", type = "float" },
                    new { name = "thumbnail", type = "string" },
                    new { name = "like_count", type = "int32" },
                    new { name = "unlike_count", type = "int32" },
                    new { name = "view_count", type = "int32" },
                    new { name = "avg_rating", type = "float" },
                    new { name = "download_count", type = "int32" },
                    new { name = "status", type = "string" },
                    new { name = "category_id", type = "int32" },
                    new { name = "seller_id", type = "int32" },
                    new { name = "created_at", type = "int64" },
                    new { name = "category_name", type = "string" },
                    new { name = "category_description", type = "string" },
                    new { name = "category_image", type = "string" }
                },
                default_sorting_field = "price"
            };

            var json = JsonConvert.SerializeObject(schema);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/collections", content);

            if (!response.IsSuccessStatusCode && (int)response.StatusCode != 409)
            {
                throw new Exception("Schema creation failed: " + await response.Content.ReadAsStringAsync());
            }
        }

        public async Task IndexProductsAsync(List<ProductModel> products)
        {
            foreach (var product in products)
            {
                var doc = new
                {
                    product_id = product.ProductId,
                    name = product.Name,
                    description = product.Description,
                    price = product.Price,
                    thumbnail = product.Thumbnail ?? "",
                    like_count = product.LikeCount,
                    unlike_count = product.UnlikeCount,
                    view_count = product.ViewCount,
                    status = product.Status ?? "",
                    category_id = product.CategoryId,
                    seller_id = product.SellerId,
                    created_at = product.CreatedAt.Ticks,
                    category_name = product.Category?.Name ?? "",
                    category_description = product.Category?.Description ?? "",
                    category_image = product.Category?.CategoryImage ?? ""
                };

                var json = JsonConvert.SerializeObject(doc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync("/collections/products/documents", content);
            }
        }

        public async Task<List<ProductModel>> SearchProductsAsync(string query)
        {
            var url = $"/collections/products/documents/search?q={query}&query_by=name,description";

            var response = await _httpClient.GetAsync(url);
            var resultJson = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TypesenseSearchResult>(resultJson);

            return result.hits.Select(h => new ProductModel
            {
                ProductId = Convert.ToInt32(h.document["product_id"]),
                Name = h.document["name"],
                Description = h.document["description"],
                Price = Convert.ToDecimal(h.document["price"]),
                Thumbnail = h.document.ContainsKey("thumbnail") ? h.document["thumbnail"] : null,
                LikeCount = h.document.ContainsKey("like_count") ? Convert.ToInt32(h.document["like_count"]) : 0,
                UnlikeCount = h.document.ContainsKey("unlike_count") ? Convert.ToInt32(h.document["unlike_count"]) : 0,
                ViewCount = h.document.ContainsKey("view_count") ? Convert.ToInt32(h.document["view_count"]) : 0,
                Status = h.document.ContainsKey("status") ? h.document["status"] : null,
                CategoryId = h.document.ContainsKey("category_id") ? Convert.ToInt32(h.document["category_id"]) : 0,
                SellerId = h.document.ContainsKey("seller_id") ? Convert.ToInt32(h.document["seller_id"]) : 0,
                CreatedAt = h.document.ContainsKey("created_at") ? new DateTime(Convert.ToInt64(h.document["created_at"])) : DateTime.MinValue,
                Category = new CategoryModel
                {
                    Name = h.document.ContainsKey("category_name") ? h.document["category_name"] : "",
                    Description = h.document.ContainsKey("category_description") ? h.document["category_description"] : "",
                    CategoryImage = h.document.ContainsKey("category_image") ? h.document["category_image"] : ""
                }
            }).ToList();
        }

        private class TypesenseSearchResult
        {
            public List<TypesenseHit> hits { get; set; }
        }

        private class TypesenseHit
        {
            public Dictionary<string, dynamic> document { get; set; }
        }
    }
}
