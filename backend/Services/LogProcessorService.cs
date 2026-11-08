using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlockchainSIEM.Models;
using BlockchainSIEM.Models.Dtos;

namespace BlockchainSIEM.Services
{
    public interface ILogProcessorService
    {
        Task<LogEntry> ProcessRawLogAsync(IngestLogRequest request);
        IEnumerable<LogEntry> GetAllLogs();
    }

    public class AiResponse
    {
        public bool IsAnomaly { get; set; }
        public double AnomalyScore { get; set; }
        public string ThreatLevel { get; set; } = "NORMAL";
    }

    public class LogProcessorService : ILogProcessorService
    {
        private static readonly ConcurrentBag<LogEntry> _logStore = new();
        private readonly HttpClient _httpClient;

        public LogProcessorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LogEntry> ProcessRawLogAsync(IngestLogRequest request)
        {
            var timestamp = DateTime.UtcNow;
            string eventType = ParseEventType(request.RawLog);

            // Call Python AI Anomaly Engine on Port 5001
            AiResponse aiResult = await QueryAiEngineAsync(request.RawLog);

            var normalizedData = new
            {
                timestamp = timestamp.ToString("o"),
                source_ip = request.SourceIp,
                log_source = request.LogSource,
                event_type = eventType,
                severity = aiResult.IsAnomaly ? "Critical" : request.Severity,
                raw = request.RawLog,
                ai_anomaly = aiResult.IsAnomaly,
                ai_score = aiResult.AnomalyScore
            };

            string normalizedJson = JsonSerializer.Serialize(normalizedData);
            string logHash = ComputeSha256(normalizedJson);

            var entry = new LogEntry
            {
                Timestamp = timestamp,
                SourceIp = request.SourceIp,
                LogSource = request.LogSource,
                EventType = aiResult.IsAnomaly ? $"{eventType}_ANOMALY" : eventType,
                Severity = aiResult.IsAnomaly ? "Critical" : request.Severity,
                RawLog = request.RawLog,
                NormalizedJson = normalizedJson,
                LogHash = logHash
            };

            _logStore.Add(entry);
            return entry;
        }

        public IEnumerable<LogEntry> GetAllLogs() => _logStore.OrderByDescending(l => l.Timestamp);

        private async Task<AiResponse> QueryAiEngineAsync(string rawLog)
        {
            try
            {
                var payload = new { rawLog = rawLog, failedAttempts = rawLog.Contains("Failed", StringComparison.OrdinalIgnoreCase) ? 10 : 0 };
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5001/api/v1/ai/detect", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AiResponse>();
                    return result ?? new AiResponse();
                }
            }
            catch
            {
                // Fallback if AI service is offline
            }
            return new AiResponse();
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