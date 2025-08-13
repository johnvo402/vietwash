using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Models;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider provider)
    {
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger>();
        var media = provider.GetRequiredService<IMediaUpdateService>();

        using var dbTransaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            string[] roles = ["ADMIN", "MANAGER", "STAFF", "CUSTOMER"];
            // Lưu Role vào DB (update nếu đã tồn tại, insert nếu chưa)

            if (!await unitOfWork.Repository<Account>().AnyAsync())
            {
                logger.Information("Seeding user data is starting.............");

                List<Account> users = await InitializeUserDataAsync(media);

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

    private static async Task<List<Account>> InitializeUserDataAsync(IMediaUpdateService media)
    {
        List<Account> users = new()
        {
            new Account(
                "Nguyễn Hửu Cảnh",
                HashPassword("Nguyencanh421510"),
                "nguyenhuucanh20032003@gmail.com",
                "0354841363",
                "CUSTOMER",
                "NV00001"
            )
            {
                BirthDay = new DateOnly(2003, 6, 15),
                Gender = Gender.Male,
            },
            new Account(
                "Võ Thanh Thư",
                HashPassword("Thư@123456"),
                "thanhthu040202@gmail.com",
                "0383395692",
                "CUSTOMER",
                "CUS000022"
            )
            {
                BirthDay = new DateOnly(2002, 2, 4),
                Gender = Gender.Male,
            },
            new Account(
                "Nguyễn Hửu Cảnh",
                HashPassword("Canh123123@"),
                "canhnhce171635@fpt.edu.vn",
                "0354841364",
                "STAFF",
                "NV00002"
            )
            {
                BirthDay = new DateOnly(2003, 10, 15),
                Gender = Gender.Female,
            },
            new Account(
                "Trần Thị Thùy Dung",
                HashPassword("thuydung123@"),
                "tranthithuydung2003@gmail.com",
                "0865125413",
                "STAFF",
                "NV00003"
            )
            {
                BirthDay = new DateOnly(2003, 6, 19),
                Gender = Gender.Female,
            },
            new Account(
                "Lê Văn Tâm",
                HashPassword("vantam1996"),
                "levantam1996@gmail.com",
                "0916558465",
                "MANAGER",
                "NV00004"
            )
            {
                BirthDay = new DateOnly(1996, 6, 5),
                Gender = Gender.Male,
            },
            new Account(
                "Trần Văn An",
                HashPassword("password1234"),
                "tranvanan2000@gmail.com",
                "0354813634",
                "CUSTOMER",
                "NV00005"
            )
            {
                BirthDay = new DateOnly(2000, 7, 15),
                Gender = Gender.Male,
            },
            new Account(
                "Nguyễn Thị Thủy",
                HashPassword("thuy1999"),
                "nguyenthithuy1999@gmail.com",
                "0987654321",
                "STAFF",
                "NV00006"
            )
            {
                BirthDay = new DateOnly(1999, 8, 20),
                Gender = Gender.Female,
            },
            new Account(
                "Lê Văn Hải",
                HashPassword("hai2001"),
                "levanhai2001@gmail.com",
                "0912345678",
                "MANAGER",
                "NV00007"
            )
            {
                BirthDay = new DateOnly(2001, 3, 10),
                Gender = Gender.Male,
            },
            new Account(
                "Phạm Thị Hồng",
                HashPassword("hong1998"),
                "phamthihong1998@gmail.com",
                "0934567890",
                "CUSTOMER",
                "NV00008"
            )
            {
                BirthDay = new DateOnly(1998, 12, 5),
                Gender = Gender.Female,
            },
            new Account(
                "Hoàng Văn Nam",
                HashPassword("nam2002"),
                "hoangvannam2002@gmail.com",
                "0976543210",
                "STAFF",
                "NV00009"
            )
            {
                BirthDay = new DateOnly(2002, 9, 15),
                Gender = Gender.Male,
            },
            new Account(
                "Đặng Thị Thảo",
                HashPassword("thao1997"),
                "dangthithao1997@gmail.com",
                "0945678901",
                "MANAGER",
                "NV00010"
            )
            {
                BirthDay = new DateOnly(1997, 4, 22),
                Gender = Gender.Female,
            },
            new Account(
                "Bùi Văn Cường",
                HashPassword("cuong2000"),
                "buivancuong2000@gmail.com",
                "0967890123",
                "CUSTOMER",
                "NV00011"
            )
            {
                BirthDay = new DateOnly(2000, 11, 30),
                Gender = Gender.Male,
            },
            new Account(
                "Nguyễn Minh Tâm",
                HashPassword("tam1999"),
                "nguyenminhtam1999@gmail.com",
                "0923456789",
                "STAFF",
                "NV00012"
            )
            {
                BirthDay = new DateOnly(1999, 6, 14),
                Gender = Gender.Male,
            },
            new Account(
                "Phạm Văn Kiệt",
                HashPassword("kiet2001"),
                "phamvankiet2001@gmail.com",
                "0989012345",
                "CUSTOMER",
                "NV00013"
            )
            {
                BirthDay = new DateOnly(2001, 1, 25),
                Gender = Gender.Male,
            },
            new Account(
                "Trịnh Thị Hạnh",
                HashPassword("hanh1996"),
                "trinhthihanh1996@gmail.com",
                "0956789012",
                "CUSTOMER",
                "NV00014"
            )
            {
                BirthDay = new DateOnly(1996, 7, 8),
                Gender = Gender.Female,
            },
            new Account(
                "Nguyễn Quang Duy",
                HashPassword("duy2002"),
                "nguyenquangduy2002@gmail.com",
                "0932109876",
                "STAFF",
                "NV00015"
            )
            {
                BirthDay = new DateOnly(2002, 10, 3),
                Gender = Gender.Male,
            },
            new Account(
                "Lê Thị Thùy Trang",
                HashPassword("trang1998"),
                "lethithuytrang1998@gmail.com",
                "0978901234",
                "CUSTOMER",
                "NV00016"
            )
            {
                BirthDay = new DateOnly(1998, 5, 17),
                Gender = Gender.Female,
            },
            new Account(
                "Hoàng Minh Quân",
                HashPassword("quan2000"),
                "hoangminhquan2000@gmail.com",
                "0943210987",
                "CUSTOMER",
                "NV00017"
            )
            {
                BirthDay = new DateOnly(2000, 8, 9),
                Gender = Gender.Male,
            },
            new Account(
                "Vũ Phi Hùng",
                HashPassword("hung1999"),
                "vuphihung1999@gmail.com",
                "0965432109",
                "CUSTOMER",
                "NV00018"
            )
            {
                BirthDay = new DateOnly(1999, 2, 28),
                Gender = Gender.Male,
            },
            new Account(
                "Trần Thị Quyên",
                HashPassword("quyen2001"),
                "tranthiquyen2001@gmail.com",
                "0921098765",
                "STAFF",
                "NV00019"
            )
            {
                BirthDay = new DateOnly(2001, 12, 12),
                Gender = Gender.Female,
            },
            new Account(
                "Nguyễn Hoàng Long",
                HashPassword("long1997"),
                "nguyenhoanglong1997@gmail.com",
                "0954321098",
                "ADMIN",
                "NV00020"
            )
            {
                BirthDay = new DateOnly(1997, 3, 15),
                Gender = Gender.Male,
            },
            new Account(
                "Nguyễn Nhật Trường",
                HashPassword("truong2003"),
                "nguyennhantruong2003@gmail.com",
                "0978468427",
                "MANAGER",
                "CUS00020"
            )
            {
                BirthDay = new DateOnly(1997, 3, 15),
                Gender = Gender.Male,
            },
        };
        var random = new Random();
        users.ForEach(u =>
        {
            u.Status = AccountStatus.Active;
            u.CreatedAt = new DateTimeOffset(
                year: 2024, // Năm 2020-2024
                month: 12, // Tháng 1-12
                day: random.Next(1, 28), // Ngày 1-27 (tránh lỗi tháng thiếu ngày)
                hour: random.Next(0, 24), // Giờ 0-23
                minute: random.Next(0, 60), // Phút 0-59
                second: random.Next(0, 60), // Giây 0-59
                offset: TimeSpan.FromHours(0)
            );
        });
        var avatarDir = Path.Combine(AppContext.BaseDirectory, "Resources", "SeedImages", "Avatar");
        var maleAvatars = Directory.EnumerateFiles(avatarDir, "male*").ToList();
        var femaleAvatars = Directory.EnumerateFiles(avatarDir, "female*").ToList();

        // Gán avatar theo giới tính
        foreach (var account in users)
        {
            var avatarList = account.Gender == Gender.Female ? femaleAvatars : maleAvatars;

            if (avatarList.Any())
            {
                var filePath = avatarList[random.Next(avatarList.Count)];
                var formFile = GenerateIFormfile(filePath);
                var key = media.GetKey(formFile, MediaType.Image);

                await media.UploadMediaAsync(formFile, key);
                account.AvtUrl = key; // Thuộc tính Avatar trong Account
            }
            else
            {
                Console.WriteLine($"[Warning] Không có avatar cho giới tính {account.Gender}.");
            }
        }
        return users;
    }

    private static IFormFile GenerateIFormfile(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        var memoryStream = new MemoryStream(fileBytes);

        var fileName = Path.GetFileName(filePath);
        var formFile = new FormFile(memoryStream, 0, fileBytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(filePath),
        };

        return formFile;
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}
