using Moq;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Domain.Repositories;

namespace UbntSecPilot.Application.Tests.Unit
{
    /// <summary>
    /// Unit tests for ThreatAnalysisService demonstrating Dependency Inversion
    /// </summary>
    public class ThreatAnalysisServiceTests
    {
        private readonly Mock<IThreatAnalysisService> _mockThreatAnalysisService;
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
            var networkEvent = new NetworkEvent("test-id", "test-source", new Dictionary<string, object>(), DateTime.UtcNow);
            var expectedFinding = new ThreatFinding("test-id", "medium", "Test threat", new Dictionary<string, object>());

            _mockThreatAnalysisService
                .Setup(s => s.AnalyzeNetworkEventAsync(networkEvent))
                .ReturnsAsync(expectedFinding);

            var handler = new AnalyzeNetworkEventHandler(_mockThreatAnalysisService.Object, _mockEventRepository.Object);

            // Act
            var result = await handler.Handle(new AnalyzeNetworkEventCommand("test-id", "test-source", new Dictionary<string, object>(), DateTime.UtcNow), CancellationToken.None);

            // Assert
            Assert.Equal(expectedFinding, result);
            _mockThreatAnalysisService.Verify(s => s.AnalyzeNetworkEventAsync(networkEvent), Times.Once);
        }
    }
}
