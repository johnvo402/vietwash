using Application.Feature.Common.Projections.Products;
using Contracts.Extensions;
using Domain.Aggregates.Products;


namespace Application.Feature.Products.Command.Update
{
	public static class UpdateProductMapping
	{
		public static void UpdateFromModel(this Product product, ProductModel model)
		{
			product.Update(
				name: model.Name,
				description: model.Description,
				sku: model.Sku,
				barcode: model.Barcode,
				image: model.Image,
				status: model.Status,
				recommendedPrice: model.RecommendedPrice
			);
			// Cập nhật ProductBranches nếu có
			if (model.ProductBranches?.Any() == true)
			{

				// Cập nhật hoặc thêm mới
				foreach (var branchModel in model.ProductBranches)
				{
					var existingBranch = product.ProductBranches
						.FirstOrDefault(pb => pb.BranchId == branchModel.BranchId);

					if (existingBranch != null)
					{
						existingBranch.Description = product.Description;
						existingBranch.Sku = product.Sku;
						existingBranch.Barcode = product.Barcode;
						existingBranch.Status = product.Status;
						existingBranch.Image = product.Image;
					}
					else
					{
						product.ProductBranches.Add(new ProductBranch
						{
							BranchId = branchModel.BranchId,
							Description = product.Description,
							Sku = product.Sku,
							Barcode = product.Barcode,
							Image = product.Image,
							Status = product.Status,
							ProductId = product.Id // optional
						});
					}
				}
			}
		}
	}
}
