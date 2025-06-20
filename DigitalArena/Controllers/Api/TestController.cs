using DigitalArena.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DigitalArena.Controllers.Api
{
    public class TestController : ApiController
    {
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            // Replace with your actual data source
            var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Sample 3D Model", Price = 19.99M },
            new Product { ProductId = 2, Name = "E-Book", Price = 9.99M }
        };

            return products;
        }
    }
}
