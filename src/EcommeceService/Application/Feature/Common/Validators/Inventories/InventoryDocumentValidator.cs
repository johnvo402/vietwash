using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Inventories;
using Contracts.Common.Messages;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Suppliers;
using FluentValidation;

namespace Application.Feature.Common.Validators.Inventories
{
    public class InventoryDocumentValidator : AbstractValidator<InventoryDocumentModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IActionAccessorService _accessorService;

        public InventoryDocumentValidator(
            IUnitOfWork unitOfWork,
            IActionAccessorService accessorService
        )
        {
            _unitOfWork = unitOfWork;
            _accessorService = accessorService;
            ApplyRules();
        }

        private void ApplyRules()
        {
            RuleFor(x => x.BranchId)
                .NotNull()
                .WithState(x =>
                    Messager
                        .Create<InventoryDocumentModel>(nameof(InventoryDocument))
                        .Property(x => x.BranchId)
                        .Message(MessageType.Null)
                        .Negative()
                        .Build()
                );

            RuleFor(x => x.Note)
                .MaximumLength(255)
                .WithState(x =>
                    Messager
                        .Create<InventoryDocumentModel>(nameof(InventoryDocument))
                        .Property(x => x.Note)
                        .Message(MessageType.MaximumLength)
                        .Build()
                );

            RuleForEach(x => x.ProductSupplyings)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ProductId)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.ProductId)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MustAsync(CheckProductExistenceAsync)
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.ProductId)
                                .Message(MessageType.Existence)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.SupplierId)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.SupplierId)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MustAsync(CheckSupplierExistenceAsync)
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.SupplierId)
                                .Message(MessageType.Existence)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.Quantity)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Price)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.Price)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.UnitRelationId)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.UnitRelationId)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MustAsync(CheckUnitRelationExistenceAsync)
                        .WithState(x =>
                            Messager
                                .Create<ProductSupplyingModel>(nameof(ProductSupplyingModel))
                                .Property(x => x.UnitRelationId)
                                .Message(MessageType.Existence)
                                .Negative()
                                .Build()
                        );
                });

            RuleForEach(x => x.EquipmentSupplyings)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.Name)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.Name)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Code)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.Code)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.Quantity)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Price)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.Price)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.Capacity)
                        .GreaterThan(0)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.Capacity)
                                .Message(MessageType.GreaterThan)
                                .Negative()
                                .Build()
                        );

                    item.RuleFor(x => x.SupplierId)
                        .NotEmpty()
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.SupplierId)
                                .Message(MessageType.Null)
                                .Negative()
                                .Build()
                        )
                        .MustAsync(CheckSupplierExistenceAsync)
                        .WithState(x =>
                            Messager
                                .Create<EquipmentSupplyingModel>(nameof(EquipmentSupplyingModel))
                                .Property(x => x.SupplierId)
                                .Message(MessageType.Existence)
                                .Negative()
                                .Build()
                        );
                });
        }

        #region Existence Check Methods

        private async Task<bool> CheckProductExistenceAsync(
            long productId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork
                .Repository<BranchProduct>()
                .AnyAsync(p => p.Id == productId, cancellationToken);
        }

        private async Task<bool> CheckSupplierExistenceAsync(
            long supplierId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork
                .Repository<Supplier>()
                .AnyAsync(s => s.Id == supplierId, cancellationToken);
        }

        private async Task<bool> CheckUnitRelationExistenceAsync(
            long unitRelationId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork
                .Repository<UnitRelation>()
                .AnyAsync(u => u.Id == unitRelationId, cancellationToken);
        }

        #endregion
    }
}
