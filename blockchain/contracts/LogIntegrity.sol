// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

contract LogIntegrity {
    struct LogRecord {
        string logId;
        bytes32 logHash;
        string deviceId;
        string severity;
        uint256 timestamp;
        address recorder;
    }

    mapping(string => LogRecord) private logs;
    mapping(string => bool) private logExists;
    
    event LogStored(string indexed logId, bytes32 indexed logHash, string deviceId, string severity, uint256 timestamp);

    function StoreHash(
        string memory _logId,
        bytes32 _logHash,
        string memory _deviceId,
        string memory _severity
    ) public {
        require(!logExists[_logId], "Log ID already registered on-chain");

        logs[_logId] = LogRecord({
            logId: _logId,
            logHash: _logHash,
            deviceId: _deviceId,
            severity: _severity,
            timestamp: block.timestamp,
            recorder: msg.sender
        });

        logExists[_logId] = true;
        emit LogStored(_logId, _logHash, _deviceId, _severity, block.timestamp);
    }

    function VerifyHash(string memory _logId, bytes32 _computedHash) public view returns (bool) {
        if (!logExists[_logId]) {
            return false;
        }
        return logs[_logId].logHash == _computedHash;
    }

    function GetLogRecord(string memory _logId) public view returns (
        bytes32 logHash,
        string memory deviceId,
        string memory severity,
        uint256 timestamp,
        address recorder
    ) {
        require(logExists[_logId], "Record does not exist");
        LogRecord memory rec = logs[_logId];
        return (rec.logHash, rec.deviceId, rec.severity, rec.timestamp, rec.recorder);
    }
}