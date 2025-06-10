using Application.Feature.Services.Queries.Detail;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Suppliers.Query.Detail
{
    public record GetSupplierDetailQuery(long SupplierId) : IRequest<GetSupplierDetailResponse>;
}
