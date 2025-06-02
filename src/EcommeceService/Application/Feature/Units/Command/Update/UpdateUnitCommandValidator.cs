using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Units;
using Domain.Aggregates.Services;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Command.Update
{
	public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IActionAccessorService accessorService;

		public UpdateUnitCommandValidator(IUnitOfWork unitOfWork, IActionAccessorService accessorService)
		{
			this.unitOfWork = unitOfWork;
			this.accessorService = accessorService;
			ApplyRules();
		}

		private void ApplyRules()
		{
			// Tái sử dụng các quy tắc từ UnitValidator

			// Kiểm tra UnitId
			RuleFor(x => x.UnitId)
				.NotEmpty()
				.WithState(x =>
					Messager
						.Create<UpdateUnitCommand>()
						.Property(x => x.UnitId)
						.Message(MessageType.Null)
						.Negative()
						.Build()
				);
				

			
		}

	}
}
