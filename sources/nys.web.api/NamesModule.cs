using nys.misc.Constants;
using nys.web.api.Services.NameGenerator;
using nys.web.api.Services.WordVariants;
using Nancy;

namespace nys.web.api
{
    public class NamesModule : NancyModule
    {
        public NamesModule(INameGeneratorService generatorService): base(Routing.ApiV1.Base + "/names")
        {
            Post["/", true] = async (ctx, ct)  =>
            {
                var name = await generatorService.Generate();
                return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(name);
            };
        }
    }
}
