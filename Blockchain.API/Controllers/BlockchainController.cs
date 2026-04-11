using Blockchain.Application.Service;
using Blockchain.Application.Validators;
using Blockchain.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blockchain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlockchainController : ControllerBase
    {
        private readonly BlockchainAppService _service;
        private readonly IBlockchainLogger _logger;

        public BlockchainController(BlockchainAppService service, IBlockchainLogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Fetch blockchain data from BlockCypher API and store in database
        /// </summary>
        /// <param name="type">Blockchain type (BTC, ETH, LTC, DASH)</param>
        /// <returns>ID of the created record</returns>
        [HttpPost("{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Fetch(string type)
        {
            try
            {
                _logger.LogInfo("Fetch request received for blockchain type: {BlockchainType}", type);

                if (!BlockchainRequestValidator.IsValidBlockchainType(type))
                {
                    var error = BlockchainRequestValidator.GetValidationError(type);
                    _logger.LogWarning("Invalid blockchain type requested: {BlockchainType}", type);
                    return BadRequest(new { message = error });
                }

                var result = await _service.FetchAndStoreAsync(type);
                return Ok(new { id = result, message = "Data fetched and stored successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argument error in Fetch: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Fetch endpoint", ex);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// Get blockchain data history
        /// </summary>
        /// <param name="type">Blockchain type (BTC, ETH, LTC, DASH)</param>
        /// <returns>List of blockchain records in descending order by CreatedAt</returns>
        [HttpGet("{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHistory(string type)
        {
            try
            {
                _logger.LogInfo("GetHistory request received for blockchain type: {BlockchainType}", type);

                if (!BlockchainRequestValidator.IsValidBlockchainType(type))
                {
                    var error = BlockchainRequestValidator.GetValidationError(type);
                    _logger.LogWarning("Invalid blockchain type requested: {BlockchainType}", type);
                    return BadRequest(new { message = error });
                }

                var result = await _service.GetHistoryAsync(type);
                return Ok(new { data = result, count = result.Count });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Argument error in GetHistory: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetHistory endpoint", ex);
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
