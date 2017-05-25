using System.Threading.Tasks;

namespace nys.web.api.Services.WordVariants
{
    public class StaticDomainZoneService : IWordVariantsService
    {
        public async Task<string[]> GetAsync()
        {
            return await Task.FromResult(new[] {"xyz", "store", "art", "fun", "site", "one", "club", "shop", "pro", "vip", "group", "top", "life", "studio", "center", "agency", "ooo", "today", "blog", "plus", "cafe", "press", "travel", "games", "wiki", "bar", "run", "me", "at", "com", "org", "pro", "tools", "ink", "fund", "fashion", "dance", "toys", "farm", "camp", "buzz", "community", "cool", "deals", "date", "eco", "foundation", "game", "net", "in", "im", "io" , "co", "cc", "fm"});
        }
    }
}