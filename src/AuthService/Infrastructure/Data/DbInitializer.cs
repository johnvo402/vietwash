using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Infrastructure.Constants;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger>();

        using var dbTransaction = await unitOfWork.CreateTransactionAsync();

        try
        {
            string[] roles = ["ADMIN", "MANAGER", "STAFF", "CUSTOMER"];
            // Lưu Role vào DB (update nếu đã tồn tại, insert nếu chưa)
            
            if (!await unitOfWork.Repository<User>().AnyAsync())
            {
                logger.Information("Seeding user data is starting.............");

                List<User> users = await InitializeUserDataAsync(unitOfWork, roles);

                foreach (var user in users)
                {
                    user.CreateUser();
                    await unitOfWork.Repository<User>().AddAsync(user);
                    await unitOfWork.SaveAsync();
                }

                logger.Information("Seeding user data has finished.............");
            }
            await unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.Information("error had occured while seeding data with {message}", ex);
            throw;
        }
    }

    private static async Task<List<User>> InitializeUserDataAsync(
        IUnitOfWork unitOfWork,
        string[] roles
    )
    {
        string sg = "79";
        string hn = "01";
        string dn = "48";

        User user = new(
            "Chloe",
            "Kim",
            "chloe.kim",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "chloe.kim@gmail.com",
            "0925123123",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1990, 10, 1),
            Status = UserStatus.Active,
            Gender = Gender.Female,
            Id = Credential.UserIds.CHLOE_KIM_ID,
        };

        User johnDoe = new(
            "John",
            "Doe",
            "john.doe",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "john.doe@example.com",
            "0803456789",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1985, 4, 23),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JOHN_DOE_ID,
        };

        User aliceSmith = new(
            "Alice",
            "Smith",
            "alice.smith",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "alice.smith@example.com",
            "0912345678",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1992, 7, 19),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ALICE_SMITH_ID,
        };

        User bobJohnson = new(
            "Bob",
            "Johnson",
            "bob.johnson",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "bob.johnson@example.com",
            "0934567890",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1980, 3, 15),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.BOB_JOHNSON_ID,
        };

        User emilyBrown = new(
            "Emily",
            "Brown",
            "emily.brown",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "emily.brown@example.com",
            "0945678901",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1995, 5, 5),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.EMILY_BROWN_ID,
        };

        User jamesWilliams = new(
            "James",
            "Williams",
            "james.williams",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "james.williams@example.com",
            "0978901234",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1983, 11, 9),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JAMES_WILLIAMS_ID,
        };

        User oliviaTaylor = new(
            "Olivia",
            "Taylor",
            "olivia.taylor",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "olivia.taylor@example.com",
            "0989012345",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1998, 2, 18),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.OLIVIA_TAYLOR_ID,
        };

        User danielLee = new(
            "Daniel",
            "Lee",
            "daniel.lee",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "daniel.lee@example.com",
            "0901234567",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1987, 9, 21),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.DANIEL_LEE_ID,
        };

        User sophiaGarcia = new(
            "Sophia",
            "Garcia",
            "sophia.garcia",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "sophia.garcia@example.com",
            "0912345679",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1994, 12, 12),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.SHOPHIA_GARCIA_ID,
        };

        User michaelMartinez = new(
            "Michael",
            "Martinez",
            "michael.martinez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "michael.martinez@example.com",
            "0913456789",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1978, 8, 8),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.MICHAEL_MARTINEZ_ID,
        };

        User isabellaHarris = new(
            "Isabella",
            "Harris",
            "isabella.harris",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "isabella.harris@example.com",
            "0945678902",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1991, 1, 1),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ISABELLA_HARRIS_ID,
        };

        User davidClark = new(
            "David",
            "Clark",
            "david.clark",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "david.clark@example.com",
            "0934567891",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1984, 6, 6),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.DAVID_CLARK_ID,
        };

        User emmaRodriguez = new(
            "Emma",
            "Rodriguez",
            "emma.rodriguez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "emma.rodriguez@example.com",
            "0956789012",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1993, 3, 3),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.EMMA_RODRIGUEZ_ID,
        };

        User andrewMoore = new(
            "Andrew",
            "Moore",
            "andrew.moore",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "andrew.moore@example.com",
            "0923456789",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1981, 10, 30),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ANDREW_MOORE_ID,
        };

        User avaJackson = new(
            "Ava",
            "Jackson",
            "ava.jackson",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "ava.jackson@example.com",
            "0935678903",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(2000, 4, 14),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.AVA_JACKSON_ID,
        };

        User joshuaWhite = new(
            "Joshua",
            "White",
            "joshua.white",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "joshua.white@example.com",
            "0914567890",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1986, 11, 17),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JOSHUA_WHITE_ID,
        };

        User charlotteThomas = new(
            "Charlotte",
            "Thomas",
            "charlotte.thomas",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "charlotte.thomas@example.com",
            "0934567892",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1997, 7, 7),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.CHARLOTTE_THOMAS_ID,
        };

        User ethanKing = new(
            "Ethan",
            "King",
            "ethan.king",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "ethan.king@example.com",
            "0923456781",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1999, 9, 9),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ETHAN_KING_ID,
        };

        User abigailScott = new(
            "Abigail",
            "Scott",
            "abigail.scott",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "abigail.scott@example.com",
            "0916789013",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1989, 2, 2),
            Status = UserStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ABIGAIL_SCOTT_ID,
        };

        User liamPerez = new(
            "Liam",
            "Perez",
            "liam.perez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "liam.perez@example.com",
            "0909876543",
            roles[new Random().Next(0, 3)]
        )
        {
            DayOfBirth = new DateTime(1988, 12, 25),
            Status = UserStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.LIAM_PEREZ_ID,
        };
        return
        [
            user,
            johnDoe,
            aliceSmith,
            bobJohnson,
            emilyBrown,
            jamesWilliams,
            oliviaTaylor,
            danielLee,
            sophiaGarcia,
            michaelMartinez,
            isabellaHarris,
            davidClark,
            emmaRodriguez,
            andrewMoore,
            avaJackson,
            joshuaWhite,
            charlotteThomas,
            ethanKing,
            abigailScott,
            liamPerez,
        ];
    }
}
