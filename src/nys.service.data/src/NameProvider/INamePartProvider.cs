using System.Threading;
using System.Threading.Tasks;

namespace nys.service.data.NameProvider
{
    public interface INamePartProvider
    {
        Task<string[]> GetAsync(CancellationToken cancellationToken);
    }
}