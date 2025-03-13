using Application.Common.Interfaces.Registers;
using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Application.Common.Interfaces
{
    public interface IServiceLaundryService : IScope
    {
        public DbSet<Service> Services { get; }
        public DbSet<Group> Groups { get; }
        public DbSet<Unit> Units { get; }

        Task CreateCategory(Category category, DbTransaction? transaction = null);

        Task CreateServiceAsync(Service service, DbTransaction? transaction = null);

        Task UpdateServiceAsync(Service service, DbTransaction? transaction = null);

        Task CreateGroupAsync(Group group, DbTransaction? transaction = null);

        Task CreateUnitAsync(Unit unit, DbTransaction? transaction = null);

        Task UpdateUnitAsync(Unit unit, DbTransaction? transaction = null);

        Task UpdateGroupAsync(Group group, IEnumerable<Service> services, DbTransaction? transaction = null);

        Task AddUnitToServiceAsync(IEnumerable<UnitRelation> units, DbTransaction? transaction = null);

    }
}
