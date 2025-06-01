using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NYCPlanner.Shared
{
	public class DropZone
	{
		public string Name { get; set; }
		public List<DropZoneItem>? Items { get; set; } = [];
	}
}
