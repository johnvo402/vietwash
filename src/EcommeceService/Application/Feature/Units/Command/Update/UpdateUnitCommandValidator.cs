using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Validators.Units;
using Contracts.Common.Messages;
using Domain.Aggregates.Services;
using FluentValidation;
using Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Units.Command.Update
{
    public class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
    {
        public UpdateUnitCommandValidator()
        {
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
