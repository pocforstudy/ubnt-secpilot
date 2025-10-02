using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;

namespace UbntSecPilot.Agents.Orleans
{
    public abstract class AgentGrain : Grain, IAgentGrain
    {
        private bool _isRunning;
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
                return new AgentResult("agent", "already running", new Dictionary<string, object>
                {
                    ["error"] = "Agent is already running"
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
                return new AgentResult("agent", "cancelled", new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _status = "failed";
                Logger.LogError(ex, "Agent {GrainId} failed", this.GetPrimaryKeyString());
                return new AgentResult("agent", "failed", new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                });
            }
            finally
            {
                _isRunning = false;
            }
        }

        protected abstract Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken);
    }
}
