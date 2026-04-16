// Observer.Api/Workspaces/ProblemCase.cs
using System;

namespace Observer.Api.Workspaces
{
    /// <summary>
    /// A problem / investigation case the engineer is working on
    /// for a specific Hunter server (via ServerUser).
    /// </summary>
    public class ProblemCase
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The (Engineer, HunterServer) workspace this case belongs to.
        /// </summary>
        public Guid ServerUserId { get; set; }

        /// <summary>
        /// Optional friendly case name ("Acme missing results", etc.).
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Company / customer name.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact name.
        /// </summary>
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Free-form description of the issue.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Case status: e.g. "Open", "InProgress", "Closed".
        /// Keeping as string for now; can enum later.
        /// </summary>
        public string Status { get; set; } = "Open";

        public DateTime CreatedUtc { get; set; }
        public DateTime? ClosedUtc { get; set; }
    }
}
