from flask import Flask, request, jsonify
from sklearn.ensemble import IsolationForest
import numpy as np

app = Flask(__name__)

# Baseline normal log vector: [raw_length, failed_attempts, character_entropy]
baseline_data = np.array([
    [120, 0, 3.2],
    [140, 1, 3.1],
    [115, 0, 3.4],
    [130, 0, 3.0],
    [150, 1, 3.3],
    [500, 15, 7.8],
    [800, 45, 8.2]
])

model = IsolationForest(contamination=0.2, random_state=42)
model.fit(baseline_data)

@app.route('/api/v1/ai/detect', methods=['POST'])
def detect_anomaly():
    data = request.json
    raw_log = data.get("rawLog", "")
    failed_attempts = int(data.get("failedAttempts", 0))

    length = len(raw_log)
    entropy = len(set(raw_log)) / float(length) if length > 0 else 0
    features = np.array([[length, failed_attempts, entropy]])

    prediction = model.predict(features)[0]
    score = float(model.decision_function(features)[0])

    is_anomaly = bool(prediction == -1)

    return jsonify({
        "isAnomaly": is_anomaly,
        "anomalyScore": round(score, 4),
        "threatLevel": "CRITICAL" if is_anomaly else "NORMAL"
    })

if __name__ == '__main__':
    print("=== Python AI Anomaly Detection Engine Active on Port 5001 ===")
    app.run(port=5001)