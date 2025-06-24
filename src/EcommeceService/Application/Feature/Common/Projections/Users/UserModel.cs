namespace Application.Features.Common.Projections.Users;

public class UserModel
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDay { get; set; }
    public string? AvtUrl { get; set; }
}
