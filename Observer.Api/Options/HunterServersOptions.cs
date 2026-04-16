using System.Collections.Generic;

namespace Observer.Api.Options
{
    public sealed class HunterServersOptions
    {
        public List<HunterServer> HunterServers { get; set; } = new();
    }

    public sealed class HunterServer
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string BaseUrl { get; set; } = "";

        // Service credentials used by Observer.Api to authenticate to Hunter.
        // These must never be returned to the client.
        public string ServiceUser { get; set; } = "";
        public string ServicePassword { get; set; } = "";
    }
}
