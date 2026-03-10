using Shared;

namespace Messaging.Abstractions
{
    /// <summary>
    /// TransportDTO (Data Transfer Object)
    /// ----
    /// This is the *fixed-framework* envelope for ALL messages that move
    /// across Hunter pipelines (Pipe, SignalR, or any future transports).
    ///
    /// Why this exists:
    ///   - Provides a single consistent contract for communication.
    ///   - Ensures every interaction is traceable (RequestId).
    ///   - Decouples transport layer (NamedPipes, SignalR, HTTP etc.)
    ///    from payload semantics.
    ///
    /// Convention:
    ///   - Observer â†’ Hunter: sends command/function to execute.
    ///   - Hunter â†’ Observer: responds with data or confirmation.
    ///
    /// Note:
    ///   - Properties are JSON-friendly so this type can be easily serialized/
    ///    deserialized across any transport.
    ///   - RequestId is stored as a Guid to guarantee uniqueness across long-lived,
    ///    distributed workflows (safer than int or string).
    /// </summary>
    public class TransportDTO
    {
        /// <summary>
        /// Logical destination for the request (e.g. "FinSvcs", "Core", "Identity").
        /// Used for multi-project routing in Hunter.
        /// </summary>
        public string? TargetProject { get; set; }

        /// <summary>
        /// Logical function or method to call.
        /// This is the "name" passed into invokers downstream.
        /// </summary>
        public string? TargetFunction { get; set; }

        /// <summary>
        /// JSON-encoded arguments for the function call.
        /// Typically array or object in string form.
        /// </summary>
        public string? Arguments { get; set; }

        /// <summary>
        /// JSON-encoded return result, if this DTO represents a response.
        /// Set by the callee/receiver.
        /// </summary>
        public string? ReturnData { get; set; }

        /// <summary>
        /// True if this TransportDTO is a response (not a request).
        /// </summary>
        public bool IsResponse { get; set; }

        /// <summary>
        /// Unique identifier that pairs request and response messages together.
        /// Critical for asynchronous, multi-call workflows.
        /// 
        /// We use Guid here rather than string/long to guarantee uniqueness
        /// across multi-day, multi-server, distributed messaging.
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// The user identity associated with this request.
        /// Used for auditing, security contexts, and user-specific routing.
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Depth starts at 0 and is incremented with each recursive call.
        /// Used to manage recursion depth limits.
        /// </summary>
        public int? Depth { get; set; }

        /// <summary>
        /// If true, this request should be delivered but no response is expected.
        /// Fire-and-forget semantics - sender does not wait for correlation.
        /// </summary>
        public bool IsFireAndForget { get; set; }

        /// <summary>
        /// If true, the sender explicitly expects a response message to be correlated
        /// back via the RequestId. Used by PipeSender to determine whether to wait
        /// for a response in the response store.
        /// </summary>
        public bool ExpectsResponse { get; set; }

        /// <summary>
        /// Optional link to "parent" DTO (e.g. when a command is spawned
        /// in response to another). Used for chained message tracking.
        /// </summary>
        public TransportDTO? Parent { get; set; }
        
        public MaskType MessageType { get; set; }

        /// <summary>
        /// If this TransportDTO represents an error response, this field
        /// carries the error message. When populated, ReturnData should
        /// generally be null.
        /// </summary>
        public string? Error { get; set; }
    }
}
