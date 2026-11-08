namespace BlockchainSIEM.Models
{
    public class LogEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string SourceIp { get; set; } = string.Empty;
        public string DestinationIp { get; set; } = "10.0.0.1";
        public string LogSource { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string RawLog { get; set; } = string.Empty;
        public string NormalizedJson { get; set; } = string.Empty;
        public string LogHash { get; set; } = string.Empty;
    }
}