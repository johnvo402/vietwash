using Application.Feature.Common.Projections.Services;
using Application.Feature.Common.Projections.Units;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Update
{
    public static class UpdateServiceMapping
    {
        public static void FromUpdateModel(this Service entity, ServiceModel model)
        {
            entity.Update(
                name: model.Name,
                status: model.Status,
                categoryId: model.CategoryId,
                branchId: model.BranchId,
                type: model.Type,
                description: model.Description,
                image: model.Image
            );
        }
    }
}
