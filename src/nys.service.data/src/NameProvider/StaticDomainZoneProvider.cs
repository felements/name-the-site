using System.Threading;
using System.Threading.Tasks;

namespace nys.service.data.NameProvider
{
    internal class StaticDomainZoneProvider : INamePartProvider
    {
        private static readonly string[] Names = {
            "xyz", "store", "art", "fun", "site", "one", "club", "shop", "pro", "vip", "group", "top", "life", "studio",
            "center", "agency", "ooo", "today", "blog", "plus", "cafe", "press", "travel", "games", "wiki", "bar",
            "run", "me", "at", "com", "org", "pro", "tools", "ink", "fund", "fashion", "dance", "toys", "farm", "camp",
            "buzz", "community", "cool", "deals", "date", "eco", "foundation", "game", "net", "in", "im", "io", "co",
            "cc", "fm"
        };
        
        public Task<string[]> GetAsync(CancellationToken ct) => Task.FromResult(Names);
    }
}