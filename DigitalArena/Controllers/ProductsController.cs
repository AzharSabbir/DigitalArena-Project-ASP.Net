using System.Web.Http;
using DigitalArena.DBContext;

namespace DigitalArena.Controllers.Api
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private readonly DigitalArenaDBContext db = new DigitalArenaDBContext();

        [Route("test")] // Relative to RoutePrefix
        [HttpGet]
        public IHttpActionResult TestApi()
        {
            return Ok("API working");
        }
    }
}
