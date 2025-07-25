using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Equipments.Enums
{
    public enum EquipmentStatus : byte
    {
        Active = 1,
		UnderMaintenance = 2,
		UnderRepair = 3,
	}
}
