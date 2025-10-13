using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Serialization;

namespace UbntSecPilot.Agents.Orleans
{
    public abstract class AgentGrain : Grain, IAgentGrain
    {
        [Id(0)]
        private bool _isRunning;

        [Id(1)]
        private string _status = "idle";

        protected ILogger Logger { get; private set; }

        public AgentGrain(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _status = "idle";
            _isRunning = false;
            Logger.LogInformation("Agent {GrainId} activated", this.GetPrimaryKeyString());
            return base.OnActivateAsync(cancellationToken);
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Agent {GrainId} deactivated", this.GetPrimaryKeyString());
            return base.OnDeactivateAsync(reason, cancellationToken);
        }

        public Task<bool> IsRunning() => Task.FromResult(_isRunning);

        public Task<string> GetStatus() => Task.FromResult(_status);

        public async Task<AgentResult> RunAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                return new AgentResult(
                    "agent",
                    "already running",
                    new Dictionary<string, object>
                    {
                        ["error"] = "Agent is already running",
                        ["events_processed"] = 0,
                        ["findings_produced"] = 0
                    });
            }

            _isRunning = true;
            _status = "running";

            try
            {
                var result = await ExecuteAsync(cancellationToken).ConfigureAwait(false);
                _status = "completed";
                return result;
            }
            catch (OperationCanceledException)
            {
                _status = "cancelled";
                return new AgentResult(
                    "agent",
                    "cancelled",
                    new Dictionary<string, object>
                    {
                        ["events_processed"] = 0,
                        ["findings_produced"] = 0
                    });
            }
            catch (Exception ex)
            {
                _status = "failed";
                Logger.LogError(ex, "Agent {GrainId} failed", this.GetPrimaryKeyString());
                return new AgentResult(
                    "agent",
                    "failed",
                    new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["events_processed"] = 0,
                        ["findings_produced"] = 0
                    });
            }
            finally
            {
                _isRunning = false;
                _status = "idle";
            }
        }

        protected abstract Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken);
    }
}
