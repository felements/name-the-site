using nys.misc.Constants;
using Nancy;

namespace nys.web.api
{
    public class NamesModule : NancyModule
    {
        public NamesModule(): base(Routing.ApiV1.Base + "/names")
        {
            Get["/"] = _ =>
            {


                return Negotiate.WithStatusCode(HttpStatusCode.OK);
            };
        }
    }
}
