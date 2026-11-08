// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

contract LogIntegrity {
    struct LogRecord {
        string logId;
        bytes32 logHash;
        uint256 timestamp;
        string deviceId;
        string severity;
        bool isVerified;
    }

    mapping(string => LogRecord) private logRecords;
    address public owner;

    event LogStored(string indexed logId, bytes32 indexed logHash, uint256 timestamp, string deviceId);

    modifier onlyOwner() {
        require(msg.sender == owner, "Caller is not contract owner");
        _;
    }

    constructor() {
        owner = msg.sender;
    }

    function StoreHash(
        string memory _logId,
        bytes32 _logHash,
        string memory _deviceId,
        string memory _severity
    ) external onlyOwner {
        require(logRecords[_logId].timestamp == 0, "Log ID already exists");

        logRecords[_logId] = LogRecord({
            logId: _logId,
            logHash: _logHash,
            timestamp: block.timestamp,
            deviceId: _deviceId,
            severity: _severity,
            isVerified: true
        });

        emit LogStored(_logId, _logHash, block.timestamp, _deviceId);
    }

    function VerifyHash(string memory _logId, bytes32 _computedHash) external view returns (bool) {
        require(logRecords[_logId].timestamp != 0, "Log ID not found");
        return logRecords[_logId].logHash == _computedHash;
    }
}
