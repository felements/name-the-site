using Nancy;

namespace nys.web.pub
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => View["suggestions/index"];
        }
    }
}
