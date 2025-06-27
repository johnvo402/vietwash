using Domain.Aggregates.Enums;
using Domain.Aggregates.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Products
{
	public class ProductBranchProjection
	{
		public long BranchId { get; set; }
		public string? Sku { get; set; }
		public string? Barcode { get; set; }
		public string? Description { get; set; }
		public ActivationStatus Status { get; set; }
		public string? Image { get; set; }
	}
}
