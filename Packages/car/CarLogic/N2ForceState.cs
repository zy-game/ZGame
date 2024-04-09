using System;

namespace CarLogic
{
	[Serializable]
	public class N2ForceState
	{
		public int Level;

		public float Duration;

		public int PreLevel;

		public bool IsCupidGas;

		public N2StateGasType GasType;
	}
}
