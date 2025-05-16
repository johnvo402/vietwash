using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Prints;

namespace Infrastructure.Data.Configurations
{
	public class PrintTemplateConfiguration : IEntityTypeConfiguration<PrintTemplate>
	{
		public void Configure(EntityTypeBuilder<PrintTemplate> builder)
		{
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => x.Id);
		}
	}
}
