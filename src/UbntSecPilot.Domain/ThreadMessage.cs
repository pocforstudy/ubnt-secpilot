using System;

namespace UbntSecPilot.Domain.Models
{
    /// <summary>
    /// Single entry inside a discussion thread to inspect for IoCs.
    /// </summary>
    public class ThreadMessage
    {
        public string Content { get; }
        public string Author { get; }
        public DateTime? CreatedAt { get; }

        public ThreadMessage(string content, string author = null, DateTime? createdAt = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Author = author;
            CreatedAt = createdAt;
        }
    }
}
