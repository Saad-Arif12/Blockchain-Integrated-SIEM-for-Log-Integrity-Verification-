namespace BlockchainSIEM.Services
{
    public interface IBlockchainService
    {
        Task<string?> SubmitHashAsync(string logId, string sha256HashHex, string deviceId, string severity);
        Task<bool> VerifyHashAsync(string logId, string sha256HashHex);
    }
}