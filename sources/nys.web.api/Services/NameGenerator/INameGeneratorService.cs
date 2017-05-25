using System.Threading.Tasks;

namespace nys.web.api.Services.NameGenerator
{
    public interface INameGeneratorService
    {
        Task<string> Generate();
    }
}