using Application.Feature.Common.Projections.InventoryImports;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.InventoryImports.Command.Create;

public class CreateInventoryImportCommand : InventoryImportModel, IRequest<Unit>;
