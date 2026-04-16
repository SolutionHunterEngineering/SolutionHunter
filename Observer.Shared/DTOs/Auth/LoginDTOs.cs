namespace Observer.Shared.DTOs.Auth
{
    public class LoginRequest
    {
        public LoginRequest() { }

        public LoginRequest(string userName, string password)
        {
            User = userName;
            Password = password;
        }
        
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResult
    {
        public LoginResult() { }
        public LoginResult(string jwt, int serverId, int userId, string userName, int version, string[] roles)
        {
            Jwt = jwt;
            ServerId = serverId;
            UserId = userId;
            UserName = userName;
            Version = version;
            Roles = roles;
        }
        
        public string Jwt { get; set; } = "";
        public int ServerId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int Version { get; set; }
        public string[] Roles { get; set; } = System.Array.Empty<string>();
    }
}
