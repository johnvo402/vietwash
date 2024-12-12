using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Micro.Shared.Model;

namespace ProjectService.Domain.Entity;

public class Organization
{
    [Key]
    public string? OrgId { get; set; } = "DOAN";
    public JsonDocument Name { get; set; } = JsonDocument.Parse("{\"en\":\"Project\",\"vi\":\"Đồ án\"}");
    public string? Description { get; set; } = "Project Description";
    public string? Logo { get; set; } = "https://via.placeholder.com/150";
    public string? Email { get; set; } = "project@doan.com";
    public string? Phone { get; set; } = "0909090909";
    public string? Address { get; set; } = "Ninh Kieu, Can Tho";
    public OrganizationSetting? Setting { get; set; }
}
