// Observer.Api/Workspaces/ServerUser.cs
using System;

namespace Observer.Api.Workspaces
{
    /// <summary>
    /// Links an engineer (AppUser) to a specific Hunter server entry
    /// they are actively working with. One row per (EngineerUserId, HunterServerId).
    /// </summary>
    public class ServerUser
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Observer engineer's user Id (AppUser.Id, int).
        /// </summary>
        public int EngineerUserId { get; set; }

        /// <summary>
        /// Hunter server Id (the GUID string defined in appsettings HunterServers).
        /// </summary>
        public string HunterServerId { get; set; } = string.Empty;

        /// <summary>
        /// When this engineer first started working with this Hunter server (UTC).
        /// </summary>
        public DateTime CreatedUtc { get; set; }
    }
}
