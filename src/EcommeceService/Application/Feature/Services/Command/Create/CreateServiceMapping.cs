using Application.Feature.Common.Projections.Services;
using Domain.Aggregates.Services;

namespace Application.Feature.Services.Command.Create
{
    public static class CreateServiceMapping
    {
        public static Service ToEntity(this ServiceModel model)
        {
            return new Service(
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
