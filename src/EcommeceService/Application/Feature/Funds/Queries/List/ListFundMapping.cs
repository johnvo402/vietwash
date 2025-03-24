using AutoMapper;
using Domain.Aggregates.Funds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Funds.Queries.List
{
	public class ListFundMapping : Profile
	{
		public ListFundMapping()
		{
			CreateMap<Fund, ListFundResponse>();
		}
	}
}
