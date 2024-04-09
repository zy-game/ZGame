using System.Collections.Generic;

namespace CarLogic
{
	public class CarGroup
	{
		private List<CarState> members = new List<CarState>(6);

		public List<CarState> Members => members;
	}
}
