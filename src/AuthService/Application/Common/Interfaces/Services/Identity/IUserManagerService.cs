using System.Data.Common;
using Application.Common.Interfaces.Registers;
using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces.Services.Identity;

public interface IUserManagerService : IScope
{


    public DbSet<Account> Accounts { get; }

}
