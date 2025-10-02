using System;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Network flow information from UDM Pro.
    /// </summary>
    public class Flow
    {
        public string Id { get; }
        public string Source { get; }
        public string Destination { get; }
        public int SourcePort { get; }
        public int DestinationPort { get; }
        public string Protocol { get; }
        public long Bytes { get; }
        public int Packets { get; }
        public DateTime Timestamp { get; }

        public Flow(string id, string source, string destination, int sourcePort, int destinationPort,
                   string protocol, long bytes, int packets, DateTime timestamp)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
            SourcePort = sourcePort;
            DestinationPort = destinationPort;
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            Bytes = bytes;
            Packets = packets;
            Timestamp = timestamp;
        }
    }
}
