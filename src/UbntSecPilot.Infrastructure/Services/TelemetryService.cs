using System.Diagnostics.Metrics;

namespace UbntSecPilot.Infrastructure.Services
{
    public class TelemetryService
    {
        private readonly Meter _meter;

        public TelemetryService()
        {
            _meter = new Meter("UbntSecPilot.Agents", "1.0.0");
            Initialize();
        }

        // Counters
        public Counter<long> ThreatAnalysisCounter { get; private set; } = null!;
        public Counter<long> EventsProcessedCounter { get; private set; } = null!;
        public Counter<long> TwoPhaseCommitPreparedCounter { get; private set; } = null!;
        public Counter<long> TwoPhaseCommitCommittedCounter { get; private set; } = null!;
        public Counter<long> TwoPhaseCommitAbortedCounter { get; private set; } = null!;

        // Histograms
        public Histogram<double> AnalysisDuration { get; private set; } = null!;
        public Histogram<double> NetworkLatency { get; private set; } = null!;

        public void Initialize()
        {
            ThreatAnalysisCounter = _meter.CreateCounter<long>(
                "threat_analysis_total",
                description: "Total number of threat analyses performed");

            EventsProcessedCounter = _meter.CreateCounter<long>(
                "events_processed_total",
                description: "Total number of events processed");

            TwoPhaseCommitPreparedCounter = _meter.CreateCounter<long>(
                "two_phase_commit_prepared_total",
                description: "Total number of 2PC prepare operations");

            TwoPhaseCommitCommittedCounter = _meter.CreateCounter<long>(
                "two_phase_commit_committed_total",
                description: "Total number of 2PC commit operations");

            TwoPhaseCommitAbortedCounter = _meter.CreateCounter<long>(
                "two_phase_commit_aborted_total",
                description: "Total number of 2PC abort operations");

            AnalysisDuration = _meter.CreateHistogram<double>(
                "analysis_duration_seconds",
                unit: "s",
                description: "Duration of threat analysis operations in seconds");

            NetworkLatency = _meter.CreateHistogram<double>(
                "network_latency_seconds",
                unit: "s",
                description: "Network latency for external service calls");
        }

        public void RecordThreatAnalysis()
        {
            ThreatAnalysisCounter?.Add(1);
        }

        public void RecordEventsProcessed(long count)
        {
            EventsProcessedCounter?.Add(count);
        }

        public void RecordTwoPhaseCommitPrepared()
        {
            TwoPhaseCommitPreparedCounter?.Add(1);
        }

        public void RecordTwoPhaseCommitCommitted()
        {
            TwoPhaseCommitCommittedCounter?.Add(1);
        }

        public void RecordTwoPhaseCommitAborted()
        {
            TwoPhaseCommitAbortedCounter?.Add(1);
        }

        public void RecordAnalysisDuration(double durationSeconds)
        {
            AnalysisDuration?.Record(durationSeconds);
        }

        public void RecordNetworkLatency(double latencySeconds)
        {
            NetworkLatency?.Record(latencySeconds);
        }

        public void Dispose()
        {
            _meter.Dispose();
        }
    }
}
