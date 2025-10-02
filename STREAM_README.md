# 🔴 Redpanda Stream Implementation

This implementation provides real-time streaming capabilities for the UBNT SecPilot API using Redpanda (Kafka-compatible) for event streaming.

## 🚀 Features

### **Real-time Streaming**
- ✅ **Server-Sent Events (SSE)** for real-time data streaming
- ✅ **Network Events Stream** - Live network security events
- ✅ **Threat Findings Stream** - Real-time threat detection results
- ✅ **Agent Decisions Stream** - Automated security decisions
- ✅ **Test Stream** - Send test messages to verify connectivity

### **Producer & Consumer**
- ✅ **Event Producer** - Publish network events to streams
- ✅ **Finding Producer** - Publish threat findings to streams
- ✅ **Decision Producer** - Publish agent decisions to streams
- ✅ **Generic Producer** - Publish custom messages to any topic
- ✅ **Stream Consumer** - Consume messages from any topic

### **Management & Monitoring**
- ✅ **Topic Management** - Create and list available topics
- ✅ **Stream Health** - Monitor stream connectivity and health
- ✅ **Metrics Collection** - Track stream performance metrics

## 📡 Stream Endpoints

### **Real-time Streams (SSE)**
```http
GET /api/stream/events      # Network events stream
GET /api/stream/findings    # Threat findings stream
GET /api/stream/decisions   # Agent decisions stream
```

### **Management Endpoints**
```http
GET /api/stream/topics      # List available topics
POST /api/stream/test       # Send test message
GET /api/metrics/stream-health  # Check stream health
```

## 🔧 Configuration

### **appsettings.json**
```json
{
  "Redpanda": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "ubnt-secpilot-consumers"
  }
}
```

### **Environment Variables**
```bash
Redpanda__BootstrapServers=redpanda:9092
Redpanda__GroupId=ubnt-secpilot-consumers
```

### **Docker Compose**
```yaml
environment:
  - Redpanda__BootstrapServers=redpanda:9092
  - Redpanda__GroupId=ubnt-secpilot-consumers
```

## 💻 Usage Examples

### **JavaScript Client**
```javascript
// Connect to events stream
const eventSource = new EventSource('/api/stream/events');

eventSource.onmessage = function(event) {
    const data = JSON.parse(event.data);
    console.log('New event:', data);
};

// Send test message
await fetch('/api/stream/test?topic=test&message=Hello World!', {
    method: 'POST'
});
```

### **Python Client**
```python
import requests
import json

# Send test message
response = requests.post('http://localhost:8000/api/stream/test',
    params={'topic': 'test', 'message': 'Hello from Python!'})

# Get available topics
response = requests.get('http://localhost:8000/api/stream/topics')
topics = response.json()
```

### **curl Commands**
```bash
# Get available topics
curl http://localhost:8000/api/stream/topics

# Send test message
curl -X POST "http://localhost:8000/api/stream/test?topic=test&message=Hello"

# Check stream health
curl http://localhost:8000/api/metrics/stream-health
```

## 🎯 Stream Topics

### **Default Topics**
- `network-events` - Network security events
- `threat-findings` - Threat analysis results
- `agent-decisions` - Automated security decisions
- `test` - Test messages

### **Custom Topics**
You can create custom topics using the stream service:
```csharp
await _streamService.CreateTopicAsync("custom-topic", partitions: 3);
```

## 🔄 Message Format

### **Network Event Message**
```json
{
  "eventId": "evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
  "source": "udm-pro",
  "payload": {
    "indicator": "suspicious_ip",
    "ip": "192.168.1.100"
  },
  "occurredAt": "2024-01-15T10:30:00Z",
  "status": "processed",
  "priority": "high"
}
```

### **Threat Finding Message**
```json
{
  "id": "fnd_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
  "eventId": "evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
  "severity": "high",
  "summary": "Suspicious network activity detected",
  "metadata": {
    "confidence": 0.85
  },
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### **Agent Decision Message**
```json
{
  "id": "dec_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
  "action": "block_ip",
  "reason": "Blocking suspicious IP address",
  "metadata": {
    "ip": "192.168.1.100",
    "confidence": 0.9
  },
  "createdAt": "2024-01-15T10:30:00Z"
}
```

## 📊 Monitoring

### **Stream Health Check**
```bash
curl http://localhost:8000/api/metrics/stream-health
```

Response:
```json
{
  "status": "healthy",
  "topics": ["network-events", "threat-findings", "agent-decisions"],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### **System Metrics**
```bash
curl http://localhost:8000/api/metrics
```

Response:
```json
{
  "totalEvents": 1250,
  "totalFindings": 45,
  "activeTopics": 3,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## 🛠️ Development

### **Adding New Stream Topics**
```csharp
// In your service
await _streamService.CreateTopicAsync("new-topic", partitions: 1);

// Publish to new topic
await _streamService.PublishMessageAsync("new-topic", key, message);
```

### **Custom Stream Consumer**
```csharp
await _streamService.ConsumeMessagesAsync("custom-topic",
    async (key, value) =>
    {
        // Process message
        Console.WriteLine($"Received: {key} -> {value}");
    },
    cancellationToken);
```

## 🔒 Security

- Streams use the same authentication as API endpoints
- JWT tokens required for accessing streams
- Rate limiting applies to stream connections
- Client disconnect detection prevents resource leaks

## 📱 Web Interface

A real-time stream monitor is available at:
```
http://localhost:8000/stream-monitor.html
```

Features:
- Live stream visualization
- Connection status monitoring
- Message statistics
- Test message sending
- Auto-reconnect functionality

## 🚨 Error Handling

### **Common Issues**
1. **Connection Refused**: Check Redpanda is running on port 9092
2. **Authentication Failed**: Verify JWT token is valid
3. **Topic Not Found**: Ensure topic exists or auto-creation is enabled
4. **Rate Limited**: Reduce connection frequency

### **Debugging**
```bash
# Check Redpanda logs
docker logs redpanda

# Check API logs
docker logs ubnt-secpilot-api

# Test connectivity
curl http://localhost:8000/api/stream/topics
```

## 🎉 Integration Examples

### **React Component**
```jsx
function EventStream() {
    const [events, setEvents] = useState([]);

    useEffect(() => {
        const eventSource = new EventSource('/api/stream/events');

        eventSource.onmessage = (event) => {
            const data = JSON.parse(event.data);
            setEvents(prev => [...prev, data]);
        };

        return () => eventSource.close();
    }, []);

    return (
        <div>
            {events.map((event, index) => (
                <div key={index}>{JSON.stringify(event)}</div>
            ))}
        </div>
    );
}
```

### **Node.js Consumer**
```javascript
const EventSource = require('eventsource');

const eventSource = new EventSource('http://localhost:8000/api/stream/events');

eventSource.onmessage = function(event) {
    console.log('New event:', JSON.parse(event.data));
};

eventSource.onerror = function(error) {
    console.error('Stream error:', error);
};
```

## 📈 Performance

- **High Throughput**: Supports thousands of messages per second
- **Low Latency**: Sub-millisecond message delivery
- **Horizontal Scaling**: Add more Redpanda brokers as needed
- **Persistence**: Messages survive restarts and failures
- **Partitioning**: Topics can be partitioned for better performance

## 🔧 Troubleshooting

### **Stream Not Working**
1. Verify Redpanda is running: `docker ps | grep redpanda`
2. Check API connectivity: `curl http://localhost:8000/health`
3. Verify JWT token: Check Authorization header
4. Check browser console for JavaScript errors

### **Messages Not Appearing**
1. Verify topic exists: `curl http://localhost:8000/api/stream/topics`
2. Check message format: Ensure JSON serialization
3. Verify producer configuration: Check bootstrap servers
4. Check consumer group: Ensure proper group ID

This implementation provides enterprise-grade streaming capabilities with real-time monitoring, comprehensive error handling, and easy integration options! 🎯
