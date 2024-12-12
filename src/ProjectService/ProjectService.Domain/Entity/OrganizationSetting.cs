using System.Text.Json;
using Micro.Shared.Model;

namespace ProjectService.Domain.Entity;

public class OrganizationSetting : BaseEntity
{
    public JsonDocument? Settings { get; set; } = JsonDocument.Parse("{\"primaryColor\":\"#000000\",\"secondaryColor\":\"#FFFFFF\"}");
    public string? OrgId { get; set; } = "DOAN";
    public Organization? Organization { get; set; }
}
