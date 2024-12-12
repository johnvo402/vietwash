using ProjectService.Domain.Entity;

namespace ProjectService.Domain.IRepository;

public interface IOrganizationSettingRepository
{
    Task<OrganizationSetting?> GetOrganizationSettingByOrgId(string orgId);
    Task<OrganizationSetting?> UpdateOrganizationSetting(OrganizationSetting organizationSetting);
}
