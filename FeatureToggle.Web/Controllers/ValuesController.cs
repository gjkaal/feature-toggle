using System.Threading.Tasks;
using FeatureServices.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FeatureToggle.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : NiceControllerBase
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public ValuesController(
            IDbContextFactory dbContextFactory,
            ILogger<ValuesController> logger
            ) : base(logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await HandleRequest("Health", async () => {
                string user;
                using (var db = _dbContextFactory.CreateDbContext<FeatureServicesContext>("FeatureToggle.Web"))
                {
                    user = await db.CurrentUser();
                }
                return user;
            });

            
        }

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
