using System.Text.Json;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Commands.Create; // Sửa từ Users sang Accounts
using Configurations;
using Contracts.ApiWrapper;
using Domain.Aggregates.Accounts; // Sửa từ Users sang Accounts
using Domain.Aggregates.Accounts.Enums;
using Domain.Aggregates.Accounts.Specifications; // Sửa từ Users sang Accounts
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Test;

public partial class TestingFixture : TestingFixture<Program, TheDbContext>
{
    public TestingFixture()
        : base(baseUrl: "http://localhost:8080/auth/api") { }

    public async Task<Account> CreateManagerAccountAsync(IFormFile? avatar = null)
    {
        CreateAccountCommand command = new()
        {
            DisplayName = "Steave Roger",
            Password = DEFAULT_USER_PASSWORD,
            Email = "steave.roger@gmail.com",
            PhoneNumber = "0925321321",
            Role = "MANAGER",
            BirthDay = DateTime.UtcNow,
            Gender = Gender.Male,
            Status = AccountStatus.Active,
            AvtUrl = avatar != null ? "avatar_url" : null,
        };

        var account = await CreateAccountAsync(command);
        return account;
    }

    public async Task<Account> CreateNormalAccountAsync(IFormFile? avatar = null)
    {
        CreateAccountCommand command = new()
        {
            DisplayName = "Sang Tran",
            Password = DEFAULT_USER_PASSWORD,
            Email = "sang.tran@gmail.com",
            PhoneNumber = "0925123124",
            Role = "USER",
            BirthDay = DateTime.UtcNow,
            Gender = Gender.Male,
            Status = AccountStatus.Active,
            AvtUrl = avatar != null ? "avatar_url" : null,
        };

        var account = await CreateAccountAsync(command);
        return account;
    }

    public async Task<Account> CreateAccountAsync(CreateAccountCommand command)
    {
        Result<CreateAccountResponse> result = await SendAsync(command);
        CreateAccountResponse createAccountResponse = result.Value!;
        return (await FindAccountByIdAsync(createAccountResponse.Id))!;
    }

    public async Task<Account?> FindAccountByIdAsync(long accountId)
    {
        using var scope = Scope();
        IUnitOfWork? unitOfWork = scope.ServiceProvider.GetService<IUnitOfWork>();
        return await unitOfWork!
            .DynamicReadOnlyRepository<Account>()
            .FindByConditionAsync(new GetAccountByIdSpecification(accountId));
    }

    private static List<T>? Read<T>(string path)
        where T : class
    {
        using FileStream json = File.OpenRead(path);
        List<T>? datas = JsonSerializer.Deserialize<List<T>>(json);
        return datas;
    }
}
