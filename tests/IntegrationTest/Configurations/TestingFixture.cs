using Application.Features.Accounts.Commands.Create;
using Domain.Aggregates.Accounts.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using tests.IntegrationTest.Configurations.Extensions;

namespace Configurations;

public class TestingFixture<TProgram, TDbContext> : IAsyncLifetime
    where TDbContext : DbContext
    where TProgram : class
{
    private CustomWebApplicationFactory<TProgram, TDbContext>? _factory;
    private CustomWebApplicationFactory<Program, TDbContext>? _authFactory; // Factory riêng cho AuthService
    private readonly PostgreSqlDatabase<TDbContext> _database;
    private HttpClient? _client; // Client cho service chính
    private HttpClient? _authClient; // Client cho AuthService
    private readonly string? baseUrl;
    public const string DEFAULT_USER_PASSWORD = "Admin@123";
    private static long? AccountId; // Giữ long? như yêu cầu

    public TestingFixture(string baseUrl)
    {
        this.baseUrl = baseUrl;
        _database = new PostgreSqlDatabase<TDbContext>();
    }

    public async Task InitializeAsync()
    {
        await _database.InitialiseAsync();
        var connection = _database.GetConnection();
        string environmentName = _database.GetEnvironmentVariable();

        // Khởi tạo factory cho service chính
        _factory = new CustomWebApplicationFactory<TProgram, TDbContext>(
            connection,
            environmentName
        );
        CreateClient();

        // Khởi tạo factory cho AuthService
        _authFactory = new CustomWebApplicationFactory<Program, TDbContext>(
            connection,
            environmentName
        );
        CreateAuthClient();

        // Tạo tài khoản super.admin mặc định
        await CreateAdminAccountAsync();
    }

    private async Task CreateAdminAccountAsync()
    {
        string roleBase = "ADMIN";
        CreateAccountCommand command = new()
        {
            DisplayName = "Super Admin",
            Password = DEFAULT_USER_PASSWORD,
            Email = "super.admin@gmail.com",
            PhoneNumber = "0925123321",
            Role = roleBase,
            BirthDay = DateTime.UtcNow,
            Gender = Gender.Male,
            Status = AccountStatus.Active,
        };

        var result = await SendAsync(command);
        var createAccountResponse =
            result.Value ?? throw new InvalidOperationException("Tạo tài khoản thất bại");
        var account = result.Value;
        if (account == null)
            throw new InvalidOperationException(
                $"Không tìm thấy tài khoản với ID {createAccountResponse.Id}"
            );

        SetAccountId(account.Id); // Lưu ID vào AccountId
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        if (_factory != null)
            await _factory.DisposeAsync();
        if (_authFactory != null)
            await _authFactory.DisposeAsync();
        _client?.Dispose();
        _authClient?.Dispose();
    }

    public async Task ResetAsync()
    {
        await _database.ResetAsync();
        // Tạo lại tài khoản super.admin sau khi reset
        await CreateAdminAccountAsync();
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        if (_factory == null)
            throw new InvalidOperationException("Factory không được khởi tạo");

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    public IServiceScope Scope()
    {
        if (_factory == null)
            throw new InvalidOperationException("Factory không được khởi tạo");

        return _factory.Services.CreateScope();
    }

    public async Task<HttpResponseMessage> MakeRequestAsync(
        string uriString,
        HttpMethod method,
        object payload,
        string? contentType = null
    )
    {
        if (_client == null || _authClient == null)
            throw new InvalidOperationException("Client hoặc AuthClient không được khởi tạo");

        var loginPayload = new
        {
            Email = "super.admin@gmail.com",
            Password = DEFAULT_USER_PASSWORD,
        };
        var loginResponse = await _authClient.CreateRequestAsync(
            "http://localhost:8080/auth/api/accounts/login",
            HttpMethod.Post,
            loginPayload
        );
        var response = await loginResponse.ToResponse<Response<LoginResponse>>();
        string token =
            response?.Results?.Token
            ?? throw new InvalidOperationException("Đăng nhập thất bại, không lấy được token");

        // Gửi yêu cầu đến service chính
        return await _client.CreateRequestAsync(
            $"{baseUrl}/{uriString.TrimStart('/')}",
            method,
            payload,
            contentType,
            token
        );
    }

    public IServiceScope CreateScope()
    {
        if (_factory == null)
            throw new InvalidOperationException("Factory không được khởi tạo");
        return _factory.Services.CreateScope();
    }

    private void CreateClient()
    {
        if (_factory == null)
            throw new InvalidOperationException("Factory không được khởi tạo");
        _client = _factory.CreateClient();
    }

    private void CreateAuthClient()
    {
        if (_authFactory == null)
            throw new InvalidOperationException("AuthFactory không được khởi tạo");
        _authClient = _authFactory.CreateClient();
    }

    public static long? GetAccountId() => AccountId;

    public static void SetAccountId(long? id) => AccountId = id;

    public static void RemoveAccountId() => AccountId = null;
}

public record LoginResponse(string Token, string Refresh);

public class Response<T>
{
    public T? Results { get; set; }
}
