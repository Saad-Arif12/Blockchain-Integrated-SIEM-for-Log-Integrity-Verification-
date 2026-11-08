using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlockchainSIEM.Models;
using BlockchainSIEM.Models.Dtos;

namespace BlockchainSIEM.Services
{
    public interface ILogProcessorService
    {
        LogEntry ProcessRawLog(IngestLogRequest request);
    }

    public class LogProcessorService : ILogProcessorService
    {
        public LogEntry ProcessRawLog(IngestLogRequest request)
        {
            var timestamp = DateTime.UtcNow;
            string eventType = ParseEventType(request.RawLog);

            var normalizedData = new
            {
                timestamp = timestamp.ToString("o"),
                source_ip = request.SourceIp,
                log_source = request.LogSource,
                event_type = eventType,
                severity = request.Severity,
                raw = request.RawLog
            };

            string normalizedJson = JsonSerializer.Serialize(normalizedData);
            string logHash = ComputeSha256(normalizedJson);

            return new LogEntry
            {
                Timestamp = timestamp,
                SourceIp = request.SourceIp,
                LogSource = request.LogSource,
                EventType = eventType,
                Severity = request.Severity,
                RawLog = request.RawLog,
                NormalizedJson = normalizedJson,
                LogHash = logHash
            };
        }

        private static string ParseEventType(string rawLog)
        {
            if (rawLog.Contains("Failed password", StringComparison.OrdinalIgnoreCase))
                return "AUTH_FAILURE";
            if (rawLog.Contains("Accepted password", StringComparison.OrdinalIgnoreCase))
                return "AUTH_SUCCESS";
            if (rawLog.Contains("UNION SELECT", StringComparison.OrdinalIgnoreCase) || rawLog.Contains("<script>", StringComparison.OrdinalIgnoreCase))
                return "WEB_ATTACK";
            return "GENERAL_EVENT";
        }

        private static string ComputeSha256(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}