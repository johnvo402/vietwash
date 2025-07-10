using System.Data.Common;
using Microsoft.EntityFrameworkCore.Update;

namespace Configurations
{
    public interface IDatabase
    {
        Task InitialiseAsync();
        DbConnection GetConnection();
        string GetConnectionString();
        string GetEnvironmentVariable();
        Task ResetAsync();
        Task DisposeAsync();
    }
}
