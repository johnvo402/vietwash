using Contracts.Application.Common;
using Domain.Aggregates.Products.Enums;
using Domain.Aggregates.Suppliers;
using FluentEmail.Core.Models;
using FluentEmail.Core;
using Domain.Aggregates.Products;


namespace Application.Feature.Common.Projections.Products
{
	public class ProductProjection : BaseResponse
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Sku { get; set; }
		public ProductStatus Status { get; set; }
		public string Barcode { get; set; }
		public decimal RecommendedPrice { get; set; }


		public virtual void MappingFrom(Product product)
		{
			Id = product.Id;
			PublicId = product.PublicId;
			CreatedAt = product.CreatedAt;
			CreatedBy = product.CreatedBy;
			UpdatedAt = product.UpdatedAt;
			UpdatedBy = product.UpdatedBy;

			Name = product.Name;
			Description = product.Description;
			Sku = product.Sku;
			Name = product.Name;
			Status = product.Status;
			Barcode = product.Barcode;
			RecommendedPrice = product.RecommendedPrice;

		}
	}
}
