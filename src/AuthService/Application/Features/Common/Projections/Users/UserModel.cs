using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Projections.Users;

public class UserModel
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }

    public string? ProvinceId { get; set; }

    public string? DistrictId { get; set; }

    public string? CommuneId { get; set; }

    public string? Street { get; set; }

    public IFormFile? Avatar { get; set; }
}
