using System.Collections.Generic;
using System.Threading.Tasks;

namespace UbntSecPilot.Application.Services
{
    public interface IUdmProService
    {
        Task<List<Dictionary<string, object>>> GetFirewallRulesAsync();
        Task<Dictionary<string, object>> CreateFirewallRuleAsync(Dictionary<string, object> ruleData);
        Task<Dictionary<string, object>> UpdateFirewallRuleAsync(string ruleId, Dictionary<string, object> updates);
        Task DeleteFirewallRuleAsync(string ruleId);
    }

    public interface IVirusTotalService
    {
        Task<Dictionary<string, object>> AnalyzeIndicatorAsync(string indicator);
        Task<Dictionary<string, object>> GetQuotaStatusAsync();
    }
}
