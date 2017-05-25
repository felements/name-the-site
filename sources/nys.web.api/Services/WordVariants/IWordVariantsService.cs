using System.Threading.Tasks;

namespace nys.web.api.Services.WordVariants
{
    public interface IWordVariantsService
    {
        Task<string[]> GetAsync();
    }
}