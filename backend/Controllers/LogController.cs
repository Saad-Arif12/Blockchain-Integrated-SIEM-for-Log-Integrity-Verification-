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
        private readonly IBlockchainService _blockchainService;

        public LogController(ILogProcessorService logProcessor, IBlockchainService blockchainService)
        {
            _logProcessor = logProcessor;
            _blockchainService = blockchainService;
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            return Ok(_logProcessor.GetAllLogs());
        }

        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest([FromBody] IngestLogRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var processedLog = await _logProcessor.ProcessRawLogAsync(request);

            string? txHash = await _blockchainService.SubmitHashAsync(
                processedLog.Id.ToString(),
                processedLog.LogHash,
                processedLog.SourceIp,
                processedLog.Severity
            );

            return Ok(new
            {
                status = "ingested",
                logId = processedLog.Id,
                eventType = processedLog.EventType,
                severity = processedLog.Severity,
                sha256Hash = processedLog.LogHash,
                timestamp = processedLog.Timestamp,
                blockchainStatus = txHash != null ? "on-chain" : "offline-buffered",
                blockchainTxHash = txHash
            });
        }

        [HttpGet("verify/{logId}/{sha256Hash}")]
        public async Task<IActionResult> Verify(string logId, string sha256Hash)
        {
            bool isVerified = await _blockchainService.VerifyHashAsync(logId, sha256Hash);

            return Ok(new
            {
                logId,
                computedHash = sha256Hash,
                isIntegrityValid = isVerified
            });
        }
    }
}