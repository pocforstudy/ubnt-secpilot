using System.Collections.Concurrent;

namespace UbntSecPilot.Application.Services
{
    public class TraceVisualizationService
    {
        private readonly ConcurrentDictionary<string, TraceEntry> _traces = new();

        public void AddTrace(string traceId, string operation, string details, string status = "running")
        {
            var entry = new TraceEntry
            {
                TraceId = traceId,
                Operation = operation,
                Details = details,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            _traces[traceId] = entry;
        }

        public void UpdateTraceStatus(string traceId, string status)
        {
            if (_traces.TryGetValue(traceId, out var entry))
            {
                entry.Status = status;
                entry.Timestamp = DateTime.UtcNow;
            }
        }

        public IEnumerable<TraceEntry> GetAllTraces()
        {
            return _traces.Values.OrderByDescending(t => t.Timestamp);
        }

        public TraceEntry? GetTrace(string traceId)
        {
            _traces.TryGetValue(traceId, out var entry);
            return entry;
        }

        public void ClearOldTraces(int hoursToKeep = 24)
        {
            var cutoff = DateTime.UtcNow.AddHours(-hoursToKeep);
            var keysToRemove = _traces.Where(kvp => kvp.Value.Timestamp < cutoff)
                                     .Select(kvp => kvp.Key)
                                     .ToList();

            foreach (var key in keysToRemove)
            {
                _traces.TryRemove(key, out _);
            }
        }
    }

    public class TraceEntry
    {
        public string TraceId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
