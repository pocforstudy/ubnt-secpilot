// MongoDB initialization script for UBNT SecPilot
db = db.getSiblingDB('UbntSecPilot');

// Create collections with appropriate indexes
db.createCollection('NetworkEvents');
db.NetworkEvents.createIndex({ "EventId": 1 }, { unique: true });
db.NetworkEvents.createIndex({ "Source": 1 });
db.NetworkEvents.createIndex({ "Status": 1 });
db.NetworkEvents.createIndex({ "OccurredAt": 1 });

db.createCollection('ThreatFindings');
db.ThreatFindings.createIndex({ "Id": 1 }, { unique: true });
db.ThreatFindings.createIndex({ "EventId": 1 });
db.ThreatFindings.createIndex({ "Severity": 1 });
db.ThreatFindings.createIndex({ "CreatedAt": 1 });

db.createCollection('AgentDecisions');
db.AgentDecisions.createIndex({ "Id": 1 }, { unique: true });
db.AgentDecisions.createIndex({ "Agent": 1 });
db.AgentDecisions.createIndex({ "CreatedAt": 1 });

db.createCollection('OutboxRecords');
db.OutboxRecords.createIndex({ "TxId": 1, "ParticipantKey": 1 }, { unique: true });
db.OutboxRecords.createIndex({ "Status": 1 });

// Insert sample data for testing
db.NetworkEvents.insertOne({
    "EventId": "sample-event-001",
    "Source": "test-sensor",
    "Payload": {
        "source_ip": "192.168.1.100",
        "destination_port": 80,
        "user_agent": "Mozilla/5.0 (compatible; test-client)"
    },
    "OccurredAt": new Date(),
    "Status": "New",
    "Priority": "Medium"
});

print("UBNT SecPilot database initialized successfully");
