using System;

namespace CarLogic
{
	public class CarSkillDipatcher : Singleton<CarSkillDipatcher>, IInit
	{
		public enum TechniqueType
		{
			None = -1,
			NoCollision = 1,
			PerfectDirft = 2,
			StartQte = 3,
			DoubleGas = 4,
			GroundGas = 5,
			GroundDirftGas = 6,
			AirGas = 7,
			AirQte = 8,
			SuperAirQte = 9,
			RankUp = 10,
			MutilRankUp = 11,
			DragDirft = 12,
			DirftGas = 13,
			DirftGasBigGas = 14,
			MutilBigGas = 15,
			GroundCollect = 16,
			FastSpeed = 17
		}

		public Action OnFastSpeedComplete;

		public Action OnDragDirftComplete;

		public Action OnStartQteComplete;

		public Action OnAirQteComplete;

		public Action OnRankUpComplete;

		public Action OnPerfectDirftComplete;

		public Action OnNoCollisionComplete;

		public void Init()
		{
		}

		public void OnComplete(int Type)
		{
			switch (Type)
			{
				case 17:
					if (OnFastSpeedComplete != null)
					{
						OnFastSpeedComplete();
					}
					break;
				case 12:
					if (OnDragDirftComplete != null)
					{
						OnDragDirftComplete();
					}
					break;
				case 3:
					if (OnStartQteComplete != null)
					{
						OnStartQteComplete();
					}
					break;
				case 8:
				case 9:
					if (OnAirQteComplete != null)
					{
						OnAirQteComplete();
					}
					break;
				case 10:
				case 11:
					if (OnRankUpComplete != null)
					{
						OnRankUpComplete();
					}
					break;
				case 2:
					if (OnPerfectDirftComplete != null)
					{
						OnPerfectDirftComplete();
					}
					break;
				case 1:
					if (OnNoCollisionComplete != null)
					{
						OnNoCollisionComplete();
					}
					break;
				case 4:
				case 5:
				case 6:
				case 7:
				case 13:
				case 14:
				case 15:
				case 16:
					break;
			}
		}
	}
}
