using System.ComponentModel.DataAnnotations;

namespace BlockchainSIEM.Models.Dtos
{
    public class IngestLogRequest
    {
        [Required]
        public string LogSource { get; set; } = "Syslog";

        [Required]
        public string SourceIp { get; set; } = "127.0.0.1";

        [Required]
        public string Severity { get; set; } = "Low";

        [Required]
        public string RawLog { get; set; } = string.Empty;
    }
}