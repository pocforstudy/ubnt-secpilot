using UbntSecPilot.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using UbntSecPilot.Agents.Orleans;

namespace UbntSecPilot.BlazorUI.Components
{
    public partial class OrleansAgentManager : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;

        private List<string> _availableAgents = new();
        private Dictionary<string, AgentStatus> _agentStatuses = new();
        private Dictionary<string, AgentResult> _agentResults = new();
        private bool _isLoading = false;
        private string _selectedAgent = "";
        private bool _isRunningAgent = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadAvailableAgents();
            await LoadAgentStatuses();
        }

        private async Task LoadAvailableAgents()
        {
            try
            {
                var agents = await Http.GetFromJsonAsync<List<string>>("/api/orleans/agents");
                _availableAgents = agents?.ToList() ?? new List<string>();
                _selectedAgent = _availableAgents.FirstOrDefault() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading available agents: {ex.Message}");
            }
        }

        private async Task LoadAgentStatuses()
        {
            try
            {
                _isLoading = true;
                foreach (var agent in _availableAgents)
                {
                    // GET /api/orleans/agents/{agentName}/status
                    var status = await Http.GetFromJsonAsync<AgentStatus>($"/api/orleans/agents/{Uri.EscapeDataString(agent)}/status");
                    if (status == null)
                    {
                        status = new AgentStatus(agent, "unknown", false);
                    }
                    _agentStatuses[agent] = status;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading agent statuses: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task RunSelectedAgent()
        {
            if (string.IsNullOrEmpty(_selectedAgent))
                return;

            try
            {
                _isRunningAgent = true;
                // POST /api/orleans/agents/{agentName}/run
                var response = await Http.PostAsync($"/api/orleans/agents/{Uri.EscapeDataString(_selectedAgent)}/run", content: null);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<AgentResult>()
                             ?? new AgentResult(_selectedAgent, "no-reason", new Dictionary<string, object>());
                _agentResults[_selectedAgent] = result;
                await LoadAgentStatuses(); // Refresh statuses
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running agent {_selectedAgent}: {ex.Message}");
            }
            finally
            {
                _isRunningAgent = false;
            }
        }

        private async Task RefreshAll()
        {
            await LoadAvailableAgents();
            await LoadAgentStatuses();
        }

        private string GetStatusBadgeClass(AgentStatus status)
        {
            return status.Status.ToLower() switch
            {
                "running" => "danger",
                "completed" => "success",
                "idle" => "secondary",
                "failed" => "warning",
                _ => "info"
            };
        }

        private string GetStatusIcon(AgentStatus status)
        {
            return status.Status.ToLower() switch
            {
                "running" => "🔄",
                "completed" => "✅",
                "idle" => "⏸️",
                "failed" => "❌",
                _ => "ℹ️"
            };
        }
    }
}
