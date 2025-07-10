using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Features.Accounts.Commands.Create;
using AutoFixture;
using Contracts.ApiWrapper;
using Domain.Aggregates.Accounts.Enums;
using Shouldly;

namespace AuthService.Test.Accounts.Create
{
    [Collection(nameof(TestingCollectionFixture))]
    public class CreateAccountHandlerTest(TestingFixture testingFixture) : IAsyncLifetime
    {
        private readonly Fixture fixture = new();
        private CreateAccountCommand command = new();

        [Fact]
        public async Task CreateAccount_ShouldCreateSuccess()
        {
            //arrage
            command.BirthDay = DateTime.UtcNow;
            command.AvtUrl = null;
            command.Gender = Gender.Male;
            command.BranchAccounts = null;
            command.Password = "Admin@123";
            command.Status = AccountStatus.Active;

            //act
            var responseApi = await testingFixture.MakeRequestAsync(
                uriString: "/accounts",
                method: HttpMethod.Post,
                payload: command
            );
            var json = await responseApi.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<CreateAccountResponse>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                }
            );
            result.ShouldNotBeNull();
            result.Status.ShouldBe(200);

            var response = result.Results!;
            var user = await testingFixture.FindAccountByIdAsync(response.Id);
            user.ShouldNotBeNull();

            user!.ShouldSatisfyAllConditions(
                () => user.Id.ShouldBe(response.Id),
                () => user.DisplayName.ShouldBe(response.DisplayName),
                () => user.Email.ShouldBe(response.Email),
                () => user.PhoneNumber.ShouldBe(response.PhoneNumber),
                () => user.BirthDay.ShouldBe(response.BirthDay),
                () => user.Gender.ShouldBe(response.Gender),
                () => user.AvtUrl.ShouldBe(response.AvtUrl),
                () => user.Status.ShouldBe(response.Status)
            );
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await testingFixture.ResetAsync();
            string roleId = "ADMIN";

            command = fixture
                .Build<CreateAccountCommand>()
                .With(x => x.Role, roleId)
                .With(x => x.Email, "admin@gmail.com")
                .With(x => x.PhoneNumber, "0123456789")
                .With(x => x.DisplayName, "admin.super")
                .Create();
        }
    }
}
