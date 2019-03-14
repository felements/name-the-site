using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using nys.service.data.NameProvider;

namespace nys.http.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NamesController: Controller
    {
        private readonly INameProvider _nameProvider;

        public NamesController(INameProvider nameProvider)
        {
            _nameProvider = nameProvider;
        }
        
        [HttpGet]
        public async Task<ActionResult<string[]>> GetAsync(int? count, CancellationToken ct)
        {
            var data =  await _nameProvider.GetNext(count ?? 5, ct);
            
            return Ok(data);
        }

    }
}