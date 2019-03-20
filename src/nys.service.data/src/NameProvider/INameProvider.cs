using System.Threading;
using System.Threading.Tasks;

namespace nys.service.data.NameProvider
{
    public interface INameProvider
    {
        /// <summary>
        /// Generate next {count} name variants
        /// </summary>
        /// <param name="count">The requested quantity of name variants</param>
        /// <param name="ct"></param>
        /// <returns>Array of generated names</returns>
        Task<string[]> GetNext(int count, CancellationToken ct);
    }
}