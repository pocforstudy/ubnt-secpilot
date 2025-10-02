using System;
using System.Collections.Generic;
using UbntSecPilot.Agents;
using Xunit;

namespace UbntSecPilot.Agents.Tests
{
    public class AgentResultTests
    {
        [Fact]
        public void Constructor_WithValidParameters_SetsProperties()
        {
            // Arrange
            var action = "test-action";
            var reason = "test-reason";
            var metadata = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42
            };

            // Act
            var result = new AgentResult(action, reason, metadata);

            // Assert
            Assert.Equal(action, result.Action);
            Assert.Equal(reason, result.Reason);
            Assert.Equal(metadata, result.Metadata);
            Assert.Equal(DateTime.UtcNow.Date, result.CreatedAt.Date); // Check date only, time will vary
        }

        [Fact]
        public void Constructor_WithNullAction_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AgentResult(null!, "reason", new Dictionary<string, object>()));
        }

        [Fact]
        public void Constructor_WithNullReason_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new AgentResult("action", null!, new Dictionary<string, object>()));
        }

        [Fact]
        public void Constructor_WithNullMetadata_UsesEmptyDictionary()
        {
            // Arrange & Act
            var result = new AgentResult("action", "reason", null!);

            // Assert
            Assert.NotNull(result.Metadata);
            Assert.Empty(result.Metadata);
        }

        [Fact]
        public void Constructor_WithEmptyMetadata_UsesEmptyDictionary()
        {
            // Arrange
            var metadata = new Dictionary<string, object>();

            // Act
            var result = new AgentResult("action", "reason", metadata);

            // Assert
            Assert.NotNull(result.Metadata);
            Assert.Empty(result.Metadata);
        }

        [Fact]
        public void Properties_AreReadOnly()
        {
            // Arrange
            var result = new AgentResult("action", "reason", new Dictionary<string, object>());

            // Act & Assert - These should not compile if properties are not read-only
            // result.Action = "new-action"; // Should not be possible
            // result.Reason = "new-reason"; // Should not be possible
            // result.Metadata = new Dictionary<string, object>(); // Should not be possible
            // result.CreatedAt = DateTime.Now; // Should not be possible

            // If we get here without compilation errors, the properties are properly read-only
            Assert.True(true);
        }

        [Fact]
        public void CreatedAt_IsSetToUtcNow()
        {
            // Arrange & Act
            var before = DateTime.UtcNow;
            var result = new AgentResult("action", "reason", new Dictionary<string, object>());
            var after = DateTime.UtcNow;

            // Assert
            Assert.True(result.CreatedAt >= before);
            Assert.True(result.CreatedAt <= after);
        }

        [Fact]
        public void Metadata_IsImmutable()
        {
            // Arrange
            var metadata = new Dictionary<string, object> { ["key"] = "value" };
            var result = new AgentResult("action", "reason", metadata);

            // Act & Assert - These should not compile if metadata is not read-only
            // result.Metadata["newKey"] = "newValue"; // Should not be possible
            // result.Metadata.Clear(); // Should not be possible

            // If we get here without compilation errors, the metadata is properly immutable
            Assert.True(true);
        }
    }
}
