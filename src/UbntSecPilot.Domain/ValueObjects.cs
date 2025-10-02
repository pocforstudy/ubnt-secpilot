using System;
using System.Collections.Generic;
using UbntSecPilot.Domain.Entities;

namespace UbntSecPilot.Domain.ValueObjects
{
    /// <summary>
    /// IP Address value object
    /// </summary>
    public class IpAddress : IEquatable<IpAddress>
    {
        public string Value { get; }

        private IpAddress(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static IpAddress Create(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new DomainException("IP address cannot be empty");

            if (!System.Net.IPAddress.TryParse(ipAddress, out _))
                throw new DomainException("Invalid IP address format");

            return new IpAddress(ipAddress);
        }

        public bool Equals(IpAddress? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as IpAddress);

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override string ToString() => Value;
    }

    /// <summary>
    /// Port number value object
    /// </summary>
    public class PortNumber : IEquatable<PortNumber>
    {
        public int Value { get; }

        private PortNumber(int value)
        {
            Value = value;
        }

        public static PortNumber Create(int port)
        {
            if (port < 1 || port > 65535)
                throw new DomainException("Port number must be between 1 and 65535");

            return new PortNumber(port);
        }

        public bool Equals(PortNumber? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object? obj) => Equals(obj as PortNumber);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// Domain name value object
    /// </summary>
    public class DomainName : IEquatable<DomainName>
    {
        public string Value { get; }

        private DomainName(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static DomainName Create(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                throw new DomainException("Domain name cannot be empty");

            if (domain.Length > 253)
                throw new DomainException("Domain name too long");

            // Basic domain validation
            var parts = domain.Split('.');
            if (parts.Length < 2)
                throw new DomainException("Invalid domain name format");

            return new DomainName(domain.ToLower());
        }

        public bool Equals(DomainName? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as DomainName);

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override string ToString() => Value;
    }

    /// <summary>
    /// Hash value object (MD5, SHA1, SHA256, etc.)
    /// </summary>
    public class HashValue : IEquatable<HashValue>
    {
        public string Value { get; }
        public HashType Type { get; }

        private HashValue(string value, HashType type)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Type = type;
        }

        public static HashValue CreateMd5(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash) || hash.Length != 32)
                throw new DomainException("Invalid MD5 hash format");

            return new HashValue(hash.ToLower(), HashType.MD5);
        }

        public static HashValue CreateSha1(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash) || hash.Length != 40)
                throw new DomainException("Invalid SHA1 hash format");

            return new HashValue(hash.ToLower(), HashType.SHA1);
        }

        public static HashValue CreateSha256(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash) || hash.Length != 64)
                throw new DomainException("Invalid SHA256 hash format");

            return new HashValue(hash.ToLower(), HashType.SHA256);
        }

        public bool Equals(HashValue? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as HashValue);

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Type, StringComparer.OrdinalIgnoreCase.GetHashCode(Value));
        }

        public override string ToString() => $"{Type}:{Value}";
    }

    /// <summary>
    /// Hash type enumeration
    /// </summary>
    public enum HashType
    {
        MD5,
        SHA1,
        SHA256,
        SHA512
    }

    /// <summary>
    /// Confidence score value object
    /// </summary>
    public class ConfidenceScore : IEquatable<ConfidenceScore>
    {
        public double Value { get; }

        private ConfidenceScore(double value)
        {
            Value = value;
        }

        public static ConfidenceScore Create(double score)
        {
            if (score < 0.0 || score > 1.0)
                throw new DomainException("Confidence score must be between 0.0 and 1.0");

            return new ConfidenceScore(Math.Round(score, 3));
        }

        public bool Equals(ConfidenceScore? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Math.Abs(Value - other.Value) < 0.001;
        }

        public override bool Equals(object? obj) => Equals(obj as ConfidenceScore);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => $"{Value:P1}";

        public static implicit operator double(ConfidenceScore score) => score.Value;
        public static explicit operator ConfidenceScore(double score) => Create(score);
    }

    /// <summary>
    /// Threat severity levels
    /// </summary>
    public class ThreatSeverity : IEquatable<ThreatSeverity>
    {
        public string Value { get; }

        private ThreatSeverity(string value)
        {
            Value = value;
        }

        public static ThreatSeverity Critical { get; } = new ThreatSeverity("critical");
        public static ThreatSeverity High { get; } = new ThreatSeverity("high");
        public static ThreatSeverity Medium { get; } = new ThreatSeverity("medium");
        public static ThreatSeverity Low { get; } = new ThreatSeverity("low");
        public static ThreatSeverity Info { get; } = new ThreatSeverity("info");

        public static ThreatSeverity Create(string severity)
        {
            var normalized = severity?.ToLower().Trim();

            return normalized switch
            {
                "critical" => Critical,
                "high" => High,
                "medium" => Medium,
                "low" => Low,
                "info" => Info,
                _ => throw new DomainException($"Invalid threat severity: {severity}")
            };
        }

        public bool Equals(ThreatSeverity? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as ThreatSeverity);

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override string ToString() => Value;
    }

    /// <summary>
    /// Event source types
    /// </summary>
    public enum EventSourceType
    {
        UDMPro,
        Firewall,
        IDS,
        Endpoint,
        NetworkSensor,
        Manual
    }
}
