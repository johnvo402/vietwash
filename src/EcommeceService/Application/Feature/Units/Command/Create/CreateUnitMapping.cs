using Application.Feature.Services.Command.Create;
using AutoMapper;
using Domain.Aggregates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Units.Command.Create
{
	public class CreateUnitMapping : Profile
	{
		public CreateUnitMapping()
		{
			CreateMap<CreateUnitCommand, Unit>();
		}
	}
}
