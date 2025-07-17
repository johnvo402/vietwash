using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;

namespace Application.Jobs
{
    public class UserOnlyId
    {
        public long Id { get; set; }
        public CustomerGroup? CustomerGroup { get; set; }
    }

    public class CheckCustomerLoyal : IJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckCustomerLoyal(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync()
        {
            var userIds = await _unitOfWork
                .DynamicReadOnlyRepository<User>()
                .ListAsync(
                    new ListCustomerWithoutIncludeSpecification(CustomerGroup.Normal),
                    new QueryParamRequest(),
                    x => new UserOnlyId { Id = x.Id, CustomerGroup = x.CustomerGroup },
                    cancellationToken: default
                );

            foreach (var userId in userIds)
            {
                var orders = await _unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .ListAsync(
                        new GetOrderByCustomerIdSpecification(userId.Id),
                        new QueryParamRequest { }
                    );

                if (orders != null && orders.Count() > 0)
                {
                    var totalOrder = orders.Count();
                    var totalRevenue = orders.Sum(x => x.Total);

                    if (totalOrder >= 5 || totalRevenue > 500000)
                    {
                        // Lấy user với AsNoTracking để tránh bị tracked
                        var user = await _unitOfWork.Repository<User>().FindByIdAsync(userId);

                        if (user == null)
                            continue;

                        user.CustomerGroup = CustomerGroup.Loyal;

                        try
                        {
                            DbTransaction transaction = await _unitOfWork.BeginTransactionAsync();
                            await _unitOfWork.Repository<User>().UpdateAsync(user);
                            await _unitOfWork.SaveAsync();
                            await _unitOfWork.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await _unitOfWork.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
        }
    }
}
