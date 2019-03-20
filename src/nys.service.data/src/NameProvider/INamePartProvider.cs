using System.Threading;
using System.Threading.Tasks;

namespace nys.service.data.NameProvider
{
    public interface INamePartProvider
    {
        /// <summary>
        /// Fetch name part variants from the implementing source
        /// </summary>
        /// <returns></returns>
        Task<string[]> GetAsync(CancellationToken cancellationToken);
    }
}