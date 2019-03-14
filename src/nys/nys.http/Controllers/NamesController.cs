using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace nys.http.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NamesController: Controller
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetAsync()
        {
            await Task.Delay(3);
            
            return Ok("hello.org");
        }

    }
}