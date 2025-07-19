using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Specifications;
using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Jobs
{
    public class DeactivateExpiredVouchersJob : IJob
    {

        private readonly IUnitOfWork _unitOfWork;

        public DeactivateExpiredVouchersJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task ExecuteAsync()
        {
            var now = DateTimeOffset.Now;
            var expiredVouchers = await _unitOfWork
                        .DynamicReadOnlyRepository<Voucher>()
                        .ListAsync(
                            new GetExpiredVouchers(now),
                            new QueryParamRequest { }
                        );
            if (!expiredVouchers.Any())
                return;
            try
            {
                DbTransaction transaction = await _unitOfWork.BeginTransactionAsync();
                foreach (var voucher in expiredVouchers)
                {
                    voucher.Status = ActivationStatus.Inactive;
                    await _unitOfWork.Repository<Voucher>().UpdateAsync(voucher);
                }

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
