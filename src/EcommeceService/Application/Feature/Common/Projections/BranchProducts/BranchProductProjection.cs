using Application.Common.Security;
using Application.Feature.Common.Mapping.Categories;
using Application.Feature.Common.Mapping.Units;
using Application.Feature.Common.Projections.Units;
using Application.Feature.Services.Queries.List;
using Application.Features.Common.Projections.Users;
using Contracts.Application.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Products;

namespace Application.Feature.Common.Projections.BranchProducts
{
    public class BranchProductProjection : BaseResponse
    {
        public long BranchId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Sku { get; set; }

        [File]
        public string? Image { get; set; }
        public decimal CapitalPrice { get; set; }

        public decimal StockQuantity { get; set; } = 0;
        public ActivationStatus Status { get; set; }

        public long CategoryId { get; set; }
        public CategoryService Category { get; set; }

        public ICollection<UnitRelationProjection> UnitRelations { get; set; } = [];

        public virtual void MappingFrom(BranchProduct branchProduct)
        {
            Id = branchProduct.Id;
            PublicId = branchProduct.PublicId;
            // CreatedAt = branchProduct.CreatedAt;
            // CreatedBy = branchProduct.CreatedBy;
            // UpdatedAt = branchProduct.UpdatedAt;
            // UpdatedBy = branchProduct.UpdatedBy;

            Name = branchProduct.Name;
            Image = branchProduct.Image;
            Status = branchProduct.Status;
            CategoryId = branchProduct.CategoryId;
            Description = branchProduct.Description;
            BranchId = branchProduct.BranchId;
            CategoryId = branchProduct.CategoryId;

            Category = branchProduct.Category.ToCategoryService();
            UnitRelations = branchProduct
                .UnitRelations.Select(x => x.ToUnitRelationProjectionResponse())
                .ToList();
        }
    }

    public class BranchProductDetailProjection : BranchProductProjection
    {
        public UserDTO? CreatedUser { get; set; }
        public UserDTO? UpdatedUser { get; set; }

        public override void MappingFrom(BranchProduct branchProduct)
        {
            base.MappingFrom(branchProduct);

            CreatedAt = branchProduct.CreatedAt;
            CreatedBy = branchProduct.CreatedBy;
            UpdatedAt = branchProduct.UpdatedAt;
            UpdatedBy = branchProduct.UpdatedBy;
        }
    }
}
