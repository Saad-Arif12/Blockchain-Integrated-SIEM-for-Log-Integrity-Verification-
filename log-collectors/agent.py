import time
import random
import requests

API_URL = "http://localhost:5000/api/v1/logs/ingest"

SAMPLE_ATTACKS = [
    {"source": "SSH", "ip": "192.168.1.105", "severity": "High", "raw": "Failed password for root from 192.168.1.105 port 22 ssh2"},
    {"source": "SSH", "ip": "192.168.1.105", "severity": "High", "raw": "Failed password for admin from 192.168.1.105 port 22 ssh2"},
    {"source": "Nginx", "ip": "10.0.0.12", "severity": "Critical", "raw": "GET /index.php?id=1' UNION SELECT NULL,username,password FROM users-- HTTP/1.1 200 1024"},
    {"source": "Nginx", "ip": "10.0.0.45", "severity": "Medium", "raw": "GET /search?q=<script>alert('xss')</script> HTTP/1.1 200 512"},
    {"source": "Firewall", "ip": "172.16.0.4", "severity": "Low", "raw": "ALLOW TCP 172.16.0.4:443 -> 8.8.8.8:53"}
]

def run_agent():
    print("=== SIEM Threat Log Generator Active ===")
    while True:
        log_data = random.choice(SAMPLE_ATTACKS)
        payload = {
            "logSource": log_data["source"],
            "sourceIp": log_data["ip"],
            "severity": log_data["severity"],
            "rawLog": log_data["raw"]
        }
        try:
            res = requests.post(API_URL, json=payload, timeout=3)
            if res.status_code == 200:
                data = res.json()
                print(f"[INGESTED] {log_data['source']} | Event: {data['eventType']} | Hash: {data['sha256Hash'][:16]}...")
        except Exception as err:
            print(f"[OFFLINE] Backend API not reachable. Retrying...")
        
        time.sleep(2)

if __name__ == "__main__":
    run_agent()