using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.Specifications;

namespace Application.Jobs
{
    public class CheckCustomerLoyal : IJob
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckCustomerLoyal(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync()
        {
            IEnumerable<User> users = await _unitOfWork.Repository<User>().ListAsync();
            List<long> userIds = new List<long>();
            foreach (User user in users)
            {
                userIds.Add(user.Id);
            }
            foreach (long userId in userIds)
            {
                var order = await _unitOfWork
                    .Repository<Order>()
                    .FindByConditionAsync<List<Order>>(
                        new GetOrderByCustomerIdSpecification(userId)
                    );
                if (order != null || order?.Count() > 0)
                {
                    var totalOrder = order.Count();
                    var totalRevenue = order.Sum(x => x.Total);
                    if (totalOrder >= 5 || totalRevenue > 500000)
                    {
                        var user = await _unitOfWork
                            .Repository<User>()
                            .FindByConditionAsync(
                                new GetUserByIdWithoutIncludeSpecification(userId)
                            );
                        try
                        {
                            DbTransaction transaction = await _unitOfWork.CreateTransactionAsync();
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
