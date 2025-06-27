using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Products;
using Application.Feature.Common.Projections.Services;
using Contracts.Common.Messages;
using Domain.Aggregates.Products;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Validators.Products
{
	public class ProductValidator : AbstractValidator<ProductModel>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public ProductValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<Product>()
						.Property(x => x.Name)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				)
				.MaximumLength(256)
				.WithState(x =>
					Messager
						.Create<Product>()
						.Property(x => x.Name)
						.Message(MessageType.MaximumLength)
						.Build()
				);
			
		}
		
	}
}
