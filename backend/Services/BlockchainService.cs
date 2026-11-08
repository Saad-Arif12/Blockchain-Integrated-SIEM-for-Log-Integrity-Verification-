using System.Text.Json;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BlockchainSIEM.Services
{
    [Function("StoreHash")]
    public class StoreHashFunction : FunctionMessage
    {
        [Parameter("string", "_logId", 1)]
        public string LogId { get; set; } = string.Empty;

        [Parameter("bytes32", "_logHash", 2)]
        public byte[] LogHash { get; set; } = Array.Empty<byte>();

        [Parameter("string", "_deviceId", 3)]
        public string DeviceId { get; set; } = string.Empty;

        [Parameter("string", "_severity", 4)]
        public string Severity { get; set; } = string.Empty;
    }

    [Function("VerifyHash", "bool")]
    public class VerifyHashFunction : FunctionMessage
    {
        [Parameter("string", "_logId", 1)]
        public string LogId { get; set; } = string.Empty;

        [Parameter("bytes32", "_computedHash", 2)]
        public byte[] ComputedHash { get; set; } = Array.Empty<byte>();
    }

    public class BlockchainService : IBlockchainService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BlockchainService> _logger;
        private readonly string _rpcUrl;
        private readonly string _contractAddress;
        private readonly string _privateKey;
        private readonly bool _enabled;

        public BlockchainService(IConfiguration config, ILogger<BlockchainService> logger)
        {
            _config = config;
            _logger = logger;
            _rpcUrl = _config["Blockchain:RpcUrl"] ?? "http://127.0.0.1:8545";
            _contractAddress = _config["Blockchain:ContractAddress"] ?? string.Empty;
            _privateKey = _config["Blockchain:PrivateKey"] ?? string.Empty;
            _enabled = bool.TryParse(_config["Blockchain:Enabled"], out var result) && result;
        }

        public async Task<string?> SubmitHashAsync(string logId, string sha256HashHex, string deviceId, string severity)
        {
            if (!_enabled || string.IsNullOrEmpty(_contractAddress) || string.IsNullOrEmpty(_privateKey))
            {
                _logger.LogWarning("Blockchain integration is disabled or credentials missing.");
                return null;
            }

            try
            {
                var account = new Account(_privateKey);
                var web3 = new Web3(account, _rpcUrl);

                byte[] hashBytes = Convert.FromHexString(sha256HashHex);

                var storeMsg = new StoreHashFunction
                {
                    LogId = logId,
                    LogHash = hashBytes,
                    DeviceId = deviceId,
                    Severity = severity
                };

                var handler = web3.Eth.GetContractTransactionHandler<StoreHashFunction>();
                var receipt = await handler.SendRequestAndWaitForReceiptAsync(_contractAddress, storeMsg);
                
                _logger.LogInformation("On-chain record created for Log {LogId} | TxHash: {TxHash}", logId, receipt.TransactionHash);
                return receipt.TransactionHash;
            }
            catch (Exception ex)
            {
                _logger.LogError("Blockchain transaction failed for Log {LogId}: {Message}", logId, ex.Message);
                return null;
            }
        }

        public async Task<bool> VerifyHashAsync(string logId, string sha256HashHex)
        {
            if (!_enabled || string.IsNullOrEmpty(_contractAddress)) return false;

            try
            {
                var web3 = new Web3(_rpcUrl);
                byte[] hashBytes = Convert.FromHexString(sha256HashHex);

                var verifyMsg = new VerifyHashFunction
                {
                    LogId = logId,
                    ComputedHash = hashBytes
                };

                var handler = web3.Eth.GetContractQueryHandler<VerifyHashFunction>();
                return await handler.QueryAsync<bool>(_contractAddress, verifyMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError("Blockchain verification query failed for Log {LogId}: {Message}", logId, ex.Message);
                return false;
            }
        }
    }
}