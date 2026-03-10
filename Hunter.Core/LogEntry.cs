using Messaging.Abstractions;
using Shared;

namespace Core
{
    /// <summary>
    /// Represents a single log entry that has passed initial filtering
    /// and is ready to be queued for transmission to Observer clients.
    ///
    /// Contains all the information needed for Observer to determine
    /// which connected developers should receive this log message.
    /// </summary>
    public record LogEntry
    {
        /// <summary>
        /// The actual log message text to display to developers.
        /// Should be human-readable and contain relevant context.
        /// </summary>
        public string MessageText { get; init; } = string.Empty;

        /// <summary>
        /// Optional transport context providing project/function information.
        /// When present, enables project/function-based filtering.
        /// </summary>
        public TransportDTO? Context { get; init; }

        /// <summary>
        /// Timestamp when this log entry was created.
        /// Useful for chronological ordering and debugging timing issues.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Convenience constructor for creating log entries with all required fields.
        /// </summary>
        /// <param name="type">The log message type/category</param>
        /// <param name="messageText">The human-readable message</param>
        /// <param name="context">Optional transport context for filtering</param>
        public LogEntry(MaskType type, string messageText, TransportDTO context = null)
        {
            context.MessageType = type;
            MessageText = messageText;
            Context = context;
        }
    }
}
