using System;

namespace Messaging.Abstractions
{
    /// <summary>
    /// DTO: Represents the result of a UserQueryDTO request.
    /// This travels back across the bus inside TransportDTO.ReturnData.
    /// 
    /// Typical example:
    ///   { "UserId": 123, "UserName": "Alice", "Email": "alice@demo.com" }
    /// </summary>
    public class UserResultDTO
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The display/user name.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) e-mail address or other contact info.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
