using System.Threading.Tasks;

namespace nys.web.api.Services.DomainAvailabilityCheck
{
    public interface IDomainAvailabilityCheckService
    {
        Task<bool> Check(string name);
    }
}