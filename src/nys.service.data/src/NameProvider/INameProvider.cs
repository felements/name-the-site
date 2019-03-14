using System.Threading;
using System.Threading.Tasks;

namespace nys.service.data.NameProvider
{
    public interface INameProvider
    {
        Task<string[]> GetNext(int count, CancellationToken ct);
    }
}