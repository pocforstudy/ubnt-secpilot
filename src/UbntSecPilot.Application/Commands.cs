using MediatR;
using System.ComponentModel.DataAnnotations;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Services;
using UbntSecPilot.Domain.Entities;

namespace UbntSecPilot.Application.Commands
{
    /// <summary>
    /// Command to analyze a network event
    /// </summary>
    public class AnalyzeNetworkEventCommand : IRequest<ThreatFinding>
    {
        public string EventId { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> Payload { get; set; }
        public DateTime OccurredAt { get; set; }

        public AnalyzeNetworkEventCommand(string eventId, string source, Dictionary<string, object> payload, DateTime occurredAt)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Payload = payload ?? new Dictionary<string, object>();
            OccurredAt = occurredAt;
        }
    }

    /// <summary>
    /// Command to run threat enrichment agent
    /// </summary>
    public class RunThreatEnrichmentCommand : IRequest<AgentDecision>
    {
        public int BatchSize { get; set; } = 10;
    }

    /// <summary>
    /// Command to analyze a thread for IoCs
    /// </summary>
    public class AnalyzeThreadCommand : IRequest<ThreadAnalysis>
    {
        public List<ThreadMessage> Messages { get; set; }

        public AnalyzeThreadCommand(List<ThreadMessage> messages)
        {
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }
    }

    /// <summary>
    /// Command to create a firewall rule
    /// </summary>
    public class CreateFirewallRuleCommand : IRequest<Dictionary<string, object>>
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public string Ruleset { get; set; }

        [Required]
        public string Action { get; set; }

        [Required]
        public string Protocol { get; set; }

        public string DstAddress { get; set; }
        public string DstPort { get; set; }
        public string Description { get; set; }

        public CreateFirewallRuleCommand(string name, string ruleset, string action, string protocol)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Ruleset = ruleset ?? throw new ArgumentNullException(nameof(ruleset));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }
    }

    /// <summary>
    /// Command to update a firewall rule
    /// </summary>
    public class UpdateFirewallRuleCommand : IRequest<Dictionary<string, object>>
    {
        [Required]
        public string RuleId { get; set; }

        public string Action { get; set; }
        public string Protocol { get; set; }
        public string DstAddress { get; set; }
        public string DstPort { get; set; }
        public string Description { get; set; }
        public bool? Enabled { get; set; }

        public UpdateFirewallRuleCommand(string ruleId)
        {
            RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        }
    }

    /// <summary>
    /// Command to delete a firewall rule
    /// </summary>
    public class DeleteFirewallRuleCommand : IRequest<bool>
    {
        [Required]
        public string RuleId { get; set; }

        public DeleteFirewallRuleCommand(string ruleId)
        {
            RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        }
    }
}
