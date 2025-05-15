using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Products.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Products
{
    public class Product : AggregateRoot
    {
        public string Description { get; set; }
        public string Sku { get; set; }
        public Status Status { get; set; }
        public string Barcode { get; set; }
        public decimal RecommendedPrice { get; set; }
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
