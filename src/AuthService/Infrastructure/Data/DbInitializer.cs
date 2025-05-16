using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
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

            if (!await unitOfWork.Repository<Account>().AnyAsync())
            {
                logger.Information("Seeding user data is starting.............");

                List<Account> users = InitializeUserData(roles);

                foreach (var user in users)
                {
                    user.CreateAccount();
                    await unitOfWork.Repository<Account>().AddAsync(user);
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

    private static List<Account> InitializeUserData(string[] roles)
    {
        Account user = new(
            "Chloe Kim",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "chloe.kim@gmail.com",
            "0925123123",
            roles[new Random().Next(0, 3)],
            "000001"
        )
        {
            BirthDay = new DateOnly(1990, 10, 1),
            Status = AccountStatus.Active,
            Gender = Gender.Female,
            Id = Credential.UserIds.CHLOE_KIM_ID,
        };

        Account johnDoe = new(
            "John Doe",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "john.doe@example.com",
            "0803456789",
            roles[new Random().Next(0, 3)],
            "000002"
        )
        {
            BirthDay = new DateOnly(1985, 4, 23),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JOHN_DOE_ID,
        };

        Account aliceSmith = new(
            "Alice Smith",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "alice.smith@example.com",
            "0912345678",
            roles[new Random().Next(0, 3)],
            "000003"
        )
        {
            BirthDay = new DateOnly(1992, 7, 19),
            Status = AccountStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ALICE_SMITH_ID,
        };

        Account bobJohnson = new(
            "Bob Johnson",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "bob.johnson@example.com",
            "0934567890",
            roles[new Random().Next(0, 3)],
            "000004"
        )
        {
            BirthDay = new DateOnly(1980, 3, 15),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.BOB_JOHNSON_ID,
        };

        Account emilyBrown = new(
            "Emily Brown",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "emily.brown@example.com",
            "0945678901",
            roles[new Random().Next(0, 3)],
            "000005"
        )
        {
            BirthDay = new DateOnly(1995, 5, 5),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.EMILY_BROWN_ID,
        };

        Account jamesWilliams = new(
            "James Williams",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "james.williams@example.com",
            "0978901234",
            roles[new Random().Next(0, 3)],
            "000006"
        )
        {
            BirthDay = new DateOnly(1983, 11, 9),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JAMES_WILLIAMS_ID,
        };

        Account oliviaTaylor = new(
            "Olivia Taylor",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "olivia.taylor@example.com",
            "0989012345",
            roles[new Random().Next(0, 3)],
            "000007"
        )
        {
            BirthDay = new DateOnly(1998, 2, 18),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.OLIVIA_TAYLOR_ID,
        };

        Account danielLee = new(
            "Daniel Lee",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "daniel.lee@example.com",
            "0901234567",
            roles[new Random().Next(0, 3)],
            "000008"
        )
        {
            BirthDay = new DateOnly(1987, 9, 21),
            Status = AccountStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.DANIEL_LEE_ID,
        };

        Account sophiaGarcia = new(
            "Sophia Garcia",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "sophia.garcia@example.com",
            "0912345679",
            roles[new Random().Next(0, 3)],
            "000009"
        )
        {
            BirthDay = new DateOnly(1994, 12, 12),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.SHOPHIA_GARCIA_ID,
        };

        Account michaelMartinez = new(
            "Michael Martinez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "michael.martinez@example.com",
            "0913456789",
            roles[new Random().Next(0, 3)],
            "000010"
        )
        {
            BirthDay = new DateOnly(1978, 8, 8),
            Status = AccountStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.MICHAEL_MARTINEZ_ID,
        };

        Account isabellaHarris = new(
            "Isabella Harris",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "isabella.harris@example.com",
            "0945678902",
            roles[new Random().Next(0, 3)],
            "000011"
        )
        {
            BirthDay = new DateOnly(1991, 1, 1),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ISABELLA_HARRIS_ID,
        };

        Account davidClark = new(
            "David Clark",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "david.clark@example.com",
            "0934567891",
            roles[new Random().Next(0, 3)],
            "000012"
        )
        {
            BirthDay = new DateOnly(1984, 6, 6),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.DAVID_CLARK_ID,
        };

        Account emmaRodriguez = new(
            "Emma Rodriguez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "emma.rodriguez@example.com",
            "0956789012",
            roles[new Random().Next(0, 3)],
            "000013"
        )
        {
            BirthDay = new DateOnly(1993, 3, 3),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.EMMA_RODRIGUEZ_ID,
        };

        Account andrewMoore = new(
            "Andrew Moore",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "andrew.moore@example.com",
            "0923456789",
            roles[new Random().Next(0, 3)],
            "000014"
        )
        {
            BirthDay = new DateOnly(1981, 10, 30),
            Status = AccountStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ANDREW_MOORE_ID,
        };

        Account avaJackson = new(
            "Ava Jackson",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "ava.jackson@example.com",
            "0935678903",
            roles[new Random().Next(0, 3)],
            "000015"
        )
        {
            BirthDay = new DateOnly(2000, 4, 14),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.AVA_JACKSON_ID,
        };

        Account joshuaWhite = new(
            "Joshua White",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "joshua.white@example.com",
            "0914567890",
            roles[new Random().Next(0, 3)],
            "000016"
        )
        {
            BirthDay = new DateOnly(1986, 11, 17),
            Status = AccountStatus.Inactive,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.JOSHUA_WHITE_ID,
        };

        Account charlotteThomas = new(
            "Charlotte Thomas",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "charlotte.thomas@example.com",
            "0934567892",
            roles[new Random().Next(0, 3)],
            "000017"
        )
        {
            BirthDay = new DateOnly(1997, 7, 7),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.CHARLOTTE_THOMAS_ID,
        };

        Account ethanKing = new(
            "Ethan King",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "ethan.king@example.com",
            "0923456781",
            roles[new Random().Next(0, 3)],
            "000018"
        )
        {
            BirthDay = new DateOnly(1999, 9, 9),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ETHAN_KING_ID,
        };

        Account abigailScott = new(
            "Abigail Scott",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "abigail.scott@example.com",
            "0916789013",
            roles[new Random().Next(0, 3)],
            "000019"
        )
        {
            BirthDay = new DateOnly(1989, 2, 2),
            Status = AccountStatus.Active,
            Gender = (Gender)new Random().Next(1, 3),
            Id = Credential.UserIds.ABIGAIL_SCOTT_ID,
        };

        Account liamPerez = new(
            "Liam Perez",
            HashPassword(Credential.USER_DEFAULT_PASSWORD),
            "liam.perez@example.com",
            "0909876543",
            roles[new Random().Next(0, 3)],
            "000020"
        )
        {
            BirthDay = new DateOnly(1988, 12, 25),
            Status = AccountStatus.Inactive,
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
