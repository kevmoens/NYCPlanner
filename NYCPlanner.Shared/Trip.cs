using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYCPlanner.Shared
{
	public record Trip
	(
		string id,
		string Key,
		List<DropZone> Zones
	);
}
