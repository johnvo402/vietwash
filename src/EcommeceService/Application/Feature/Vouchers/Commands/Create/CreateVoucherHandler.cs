using System.Data.Common;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Specifications;
using Domain.Aggregates.Vouchers;
using Mediator;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.OpenApi.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Feature.Vouchers.Commands.Create
{
    public class CreateVoucherHandler(
        IUnitOfWork unitOfWork,
        IMediaUpdateService mediaUpdateService
    ) : IRequestHandler<CreateVoucherCommand, Result>
    {
        public async ValueTask<Result> Handle(
            CreateVoucherCommand request,
            CancellationToken cancellationToken
        )
        {
            Voucher mappingVoucher = request.ToEntity();

            string? voucherImage = null;
            try
            {
                DbTransaction transaction = await unitOfWork.BeginTransactionAsync(
                    cancellationToken
                );
                if (request.CustomerGroups != null && request.CustomerGroups.Any())
                {
                    var groups = request.CustomerGroups.Select(x => x.ToString()).ToList();

                    var userList = await unitOfWork
                        .DynamicReadOnlyRepository<User>()
                        .ListAsync(
                            new GetCustomerByCustomerGroups(request.CustomerGroups),
                            new QueryParamRequest(),
                            cancellationToken: cancellationToken
                        );
                    mappingVoucher.TotalQuantity = userList.Count;
                    foreach (var user in userList)
                    {

                        mappingVoucher.VoucherCustomers.Add(
                            new VoucherCustomer
                            {
                                Voucher = mappingVoucher,
                                CustomerId = user.Id,
                                IsUsed = false,
                            }
                        );
                    }
                }
                else
                {
                    var customerIds = request.CustomerIds.Select(x => x).ToList();
                    mappingVoucher.TotalQuantity = customerIds.Count;
                    foreach (var customerId in customerIds)
                    {

                        mappingVoucher.VoucherCustomers.Add(
                            new VoucherCustomer
                            {
                                Voucher = mappingVoucher,
                                CustomerId = customerId,
                                IsUsed = false,
                            }
                        );
                    }
                }

                Voucher voucher = await unitOfWork
                    .Repository<Voucher>()
                    .AddAsync(mappingVoucher, cancellationToken);
                voucherImage = voucher.ImgUrl;

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(voucherImage))
                {
                    await mediaUpdateService.DeleteAvatarAsync(voucherImage);
                }
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
