using Blockchain.Application.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blockchain.API.Controllers
{
    [ApiController]
    [Route("api/blockchain")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainAppService _service;

        public BlockchainController(BlockchainAppService service)
        {
            _service = service;
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> Fetch(string type)
        {
            var result = await _service.FetchAndStoreAsync(type);
            return Ok(result);
        }

        [HttpGet("{type}")]
        public async Task<IActionResult> GetHistory(string type)
        {
            var result = await _service.GetHistoryAsync(type);
            return Ok(result);
        }
    }
}
