using Microsoft.AspNetCore.Mvc;
using BlockchainSIEM.Models.Dtos;
using BlockchainSIEM.Services;

namespace BlockchainSIEM.Controllers
{
    [ApiController]
    [Route("api/v1/logs")]
    public class LogController : ControllerBase
    {
        private readonly ILogProcessorService _logProcessor;

        public LogController(ILogProcessorService logProcessor)
        {
            _logProcessor = logProcessor;
        }

        [HttpPost("ingest")]
        public IActionResult Ingest([FromBody] IngestLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var processedLog = _logProcessor.ProcessRawLog(request);

            return Ok(new
            {
                status = "ingested",
                logId = processedLog.Id,
                eventType = processedLog.EventType,
                sha256Hash = processedLog.LogHash,
                timestamp = processedLog.Timestamp
            });
        }
    }
}