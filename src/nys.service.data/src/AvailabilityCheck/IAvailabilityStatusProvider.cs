using System.Threading.Tasks;

namespace nys.service.data.AvailabilityCheck
{
    public interface IAvailabilityStatusProvider
    {
        Task<bool> IsAvailable(string name);
    }
}