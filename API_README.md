# UBNT SecPilot API Documentation

## Overview

The UBNT SecPilot API provides comprehensive security threat analysis and monitoring capabilities through a RESTful interface. Built with **.NET Core 8** and following clean architecture principles, the API offers endpoints for:

- **Event Management**: Collect, analyze, and retrieve network security events
- **Threat Analysis**: Process events through sophisticated threat detection algorithms
- **Agent Operations**: Execute automated threat enrichment and response agents
- **Firewall Management**: Manage UDM Pro firewall rules and policies
- **Thread Analysis**: Analyze conversation threads for indicators of compromise
- **System Monitoring**: Health checks and performance metrics

## Getting Started

### Prerequisites

- **.NET Core 8.0** or later
- **Docker** (optional, for containerized deployment)
- **MongoDB** (or SQL Server/Redis for alternative storage)

### Running the API Server

#### Option 1: Using .NET CLI
```bash
# Navigate to project root
cd /path/to/ubnt-secpilot

# Restore dependencies
dotnet restore

# Run the API server
dotnet run --project UbntSecPilot.WebApi
```

#### Option 2: Using Docker
```bash
# Build and run with Docker Compose
docker compose up --build
```

The API will be available at:
- **Local**: http://localhost:8000
- **Interactive Docs**: http://localhost:8000/swagger
- **OpenAPI Schema**: http://localhost:8000/swagger/v1/swagger.json

### Authentication

The API uses JWT (JSON Web Token) authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## API Endpoints

### Health Check

**GET** `/health`

Returns the current system health status.

**Response:**
```json
{
  "status": "ok",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Events

**GET** `/api/events`

Retrieve network security events with optional filtering.

**Parameters:**
- `limit` (optional): Maximum number of events to return (default: 100, max: 1000)
- `source` (optional): Filter by event source (e.g., "udm-pro", "firewall")
- `startTime` (optional): Start time filter (ISO 8601 format)
- `endTime` (optional): End time filter (ISO 8601 format)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "eventId": "evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
      "source": "udm-pro",
      "payload": { "indicator": "suspicious_ip", "ip": "192.168.1.100" },
      "occurredAt": "2024-01-15T10:30:00Z",
      "status": "processed",
      "priority": "high",
      "shortSummary": "udm-pro: suspicious_ip @ 2024-01-15T10:30:00Z",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "message": "Events retrieved successfully"
}
```

**GET** `/api/events/{eventId}`

Retrieve a specific network event by ID.

**POST** `/api/events/analyze`

Analyze a network event for potential security threats.

**Request Body:**
```json
{
  "eventId": "evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
  "source": "udm-pro",
  "payload": {
    "indicator": "suspicious_ip",
    "ip": "192.168.1.100"
  },
  "occurredAt": "2024-01-15T10:30:00Z"
}
```

### Findings

**GET** `/api/findings`

Retrieve threat findings with optional severity filtering.

**Parameters:**
- `limit` (optional): Maximum number of findings to return (default: 100)
- `severity` (optional): Filter by severity level (critical, high, medium, low)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "fnd_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
      "eventId": "evt_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
      "severity": "high",
      "summary": "Suspicious network activity detected from IP 192.168.1.100",
      "metadata": { "confidence": 0.85 },
      "status": "open",
      "assignedTo": "analyst@company.com",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T10:30:00Z"
    }
  ],
  "message": "Findings retrieved successfully"
}
```

### Agent Operations

**POST** `/api/agent/run`

Execute the threat enrichment agent to process unanalyzed events.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "dec_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
    "action": "block_ip",
    "reason": "Blocking suspicious IP address due to multiple failed login attempts",
    "metadata": { "ip": "192.168.1.100", "confidence": 0.9 },
    "status": "executed",
    "executedBy": "admin@company.com",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "message": "Agent executed successfully"
}
```

### Thread Analysis

**POST** `/api/analysis/thread`

Analyze a conversation thread for indicators of compromise.

**Request Body:**
```json
{
  "messages": [
    {
      "content": "Investigating possible beacon to https://c2.example.com",
      "author": "analyst",
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "content": "Detected host 10.0.4.20 contacting 203.0.113.99",
      "author": "sensor",
      "createdAt": "2024-01-15T10:31:00Z"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "ana_64f1e8b2-3c4d-5e6f-7g8h-9i0j1k2l3m4n",
    "threadId": "thread-001",
    "isIoc": true,
    "severity": "high",
    "reason": "Potential command and control communication detected",
    "indicators": ["c2_server", "suspicious_domain"],
    "metadata": { "message_count": 15, "suspicious_patterns": 3 },
    "status": "completed",
    "createdAt": "2024-01-15T10:32:00Z",
    "updatedAt": "2024-01-15T10:32:00Z"
  },
  "message": "Thread analysis completed"
}
```

### System Metrics

**GET** `/api/metrics`

Get system performance metrics (Admin only).

**Response:**
```json
{
  "success": true,
  "data": {
    "uptime": "1.02:30:45.1234567",
    "memory_usage": 134217728,
    "thread_count": 24,
    "timestamp": "2024-01-15T10:30:00Z"
  },
  "message": "System metrics retrieved"
}
```

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Field 'name' is required",
    "Invalid email format"
  ]
}
```

## Status Codes

- **200**: Success
- **201**: Created
- **400**: Bad Request (validation errors)
- **401**: Unauthorized (invalid or missing authentication)
- **403**: Forbidden (insufficient permissions)
- **404**: Not Found
- **500**: Internal Server Error

## Rate Limiting

The API implements rate limiting to prevent abuse:
- **Default Limit**: 100 requests per minute per IP
- **Headers**:
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Remaining requests in current window
  - `X-RateLimit-Reset`: Timestamp when the limit resets

## Security Features

### Authentication Methods
1. **JWT Bearer Token** (recommended)
2. **API Key** (for external integrations)

### Authorization
- **Admin Role**: Full access to all endpoints
- **User Role**: Access to read operations and limited write operations

### Security Headers
The API implements comprehensive security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains`

## Examples

### Get Events with Filtering
```bash
curl -X GET "http://localhost:8000/api/events?limit=50&source=udm-pro" \
  -H "Authorization: Bearer your-jwt-token"
```

### Analyze Network Event
```bash
curl -X POST "http://localhost:8000/api/events/analyze" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "eventId": "evt_12345",
    "source": "udm-pro",
    "payload": {
      "indicator": "suspicious_ip",
      "ip": "192.168.1.100"
    },
    "occurredAt": "2024-01-15T10:30:00Z"
  }'
```

### Run Threat Enrichment Agent
```bash
curl -X POST "http://localhost:8000/api/agent/run" \
  -H "Authorization: Bearer your-jwt-token"
```

## Interactive Documentation

The API provides interactive documentation through Swagger UI:

1. **Swagger UI**: Visit `http://localhost:8000/swagger`
2. **OpenAPI JSON**: Available at `http://localhost:8000/swagger/v1/swagger.json`

Features of the interactive documentation:
- **Try it out**: Test endpoints directly from the browser
- **Authentication**: Enter JWT tokens for authenticated requests
- **Request/Response examples**: View sample requests and responses
- **Schema validation**: Understand expected data formats
- **Error handling**: See possible error responses

## Best Practices

1. **Use HTTPS**: Always use HTTPS in production
2. **Handle Rate Limits**: Implement exponential backoff for rate-limited requests
3. **Cache Responses**: Cache frequently accessed data to reduce API calls
4. **Monitor Usage**: Track your API usage to avoid hitting rate limits
5. **Use Pagination**: For large datasets, use the `limit` and pagination parameters

## Error Handling

The API provides detailed error information to help with debugging:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The EventId field is required",
    "The Source field must be between 1 and 200 characters"
  ]
}
```

## Support

For API support and questions:
- **Email**: security@ubnt.com
- **Documentation**: Visit the Swagger UI at `/swagger`
- **GitHub Issues**: Report bugs and feature requests

## Version History

- **v1.0.0**: Initial release with core functionality
  - Event management and analysis
  - Threat findings and agent decisions
  - Firewall rule management
  - Thread analysis capabilities
  - Comprehensive security features

## License

This API is licensed under the MIT License. See the license file for details.
