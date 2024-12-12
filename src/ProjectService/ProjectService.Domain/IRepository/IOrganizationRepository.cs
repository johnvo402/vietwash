using ProjectService.Domain.Entity;

namespace ProjectService.Domain.IRepository;

public interface IOrganizationRepository
{
    Task<Organization?> GetOrganizationById(string orgId);
    Task<Organization?> CreateOrganization(Organization organization);
    Task<Organization?> UpdateOrganization(Organization organization);
    Task<Organization?> DeleteOrganization(string orgId);
}
