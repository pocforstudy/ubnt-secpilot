using Moq;
using UbntSecPilot.Domain.Services; 
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;
using UbntSecPilot.Application.Commands;
using UbntSecPilot.Application.Handlers;

namespace UbntSecPilot.Application.Tests.Unit
{
    /// <summary>
    /// Unit tests for ThreatAnalysisService demonstrating Dependency Inversion
    /// </summary>
    public class ThreatAnalysisServiceTests
    {
        private readonly Mock<IThreatAnalysisService> _mockThreatAnalysisService; // Domain service
        private readonly Mock<INetworkEventRepository> _mockEventRepository;

        public ThreatAnalysisServiceTests()
        {
            _mockThreatAnalysisService = new Mock<IThreatAnalysisService>();
            _mockEventRepository = new Mock<INetworkEventRepository>();
        }

        [Fact]
        public async Task AnalyzeNetworkEvent_Should_CallThreatAnalysisService()
        {
            // Arrange
            var payload = new Dictionary<string, object> { ["threat_level"] = "medium" };
            var networkEvent = new NetworkEvent("test-id", "test-source", payload, DateTime.UtcNow);
            var expectedFinding = new ThreatFinding("test-id", "medium", "Test threat", new Dictionary<string, object>());

            _mockThreatAnalysisService
                .Setup(s => s.AnalyzeNetworkEventAsync(It.Is<NetworkEvent>(e => e.EventId == "test-id" && e.Source == "test-source")))
                .ReturnsAsync(expectedFinding);

            var handler = new AnalyzeNetworkEventHandler(_mockThreatAnalysisService.Object, _mockEventRepository.Object);

            // Act
            var result = await handler.Handle(new AnalyzeNetworkEventCommand("test-id", "test-source", payload, DateTime.UtcNow), CancellationToken.None);

            // Assert
            Assert.Equal(expectedFinding, result);
            _mockThreatAnalysisService.Verify(s => s.AnalyzeNetworkEventAsync(It.Is<NetworkEvent>(e => e.EventId == "test-id" && e.Source == "test-source")), Times.Once);
        }
    }
}
