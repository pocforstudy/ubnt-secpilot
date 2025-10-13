using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace UbntSecPilot.Agents
{
    public abstract class BaseAgent : IAgent
    {
        public string Name { get; }

        protected BaseAgent(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public async Task<AgentResult> RunAsync(CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var (reason, metadata) = await LoopAsync(cancellationToken).ConfigureAwait(false);
                metadata ??= new Dictionary<string, object>();
                metadata["elapsed_ms"] = (double)sw.ElapsedMilliseconds;
                return new AgentResult(Name, reason, new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(metadata));
            }
            catch (OperationCanceledException)
            {
                return new AgentResult(Name, "cancelled", new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    ["elapsed_ms"] = (double)sw.ElapsedMilliseconds
                }));
            }
            catch (Exception ex)
            {
                return new AgentResult(Name, "failed", new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["elapsed_ms"] = (double)sw.ElapsedMilliseconds
                }));
            }
        }

        protected abstract Task<(string reason, IDictionary<string, object> metadata)> LoopAsync(CancellationToken cancellationToken);
    }
}
