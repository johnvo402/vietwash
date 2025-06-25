using Application.Feature.Common.Projections.Products;
using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Products.Command.Create
{
	public static class CreateProductMapping
	{
		public static Product ToEntity(this ProductModel model)
		{
			return new Product(
				name: model.Name,
				description: model.Description,
				sku: model.Sku,
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status,
				recommendedPrice: model.RecommendedPrice
			);
		}
	}
}
