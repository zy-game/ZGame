using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class DriftState
	{
		public Vector3 StartDirection = Vector3.zero;

		public float ExtraTorque;

		public float LimitDot = 1f;

		public float veldot;

		public float dirdot;

		public float StartSteer;

		public float TimePast;

		public float StartLinearVelocity;

		public float RemainStartAt;

		public float StartTime;

		public float FullRemainTime;

		public float LastGroundedTime;

		public int NoRemainFlag;

		public int GoodCount;

		public DriftStage Stage;

		public void CopyTo(DriftState des)
		{
			des.StartDirection = StartDirection;
			des.StartSteer = StartSteer;
			des.TimePast = TimePast;
			des.StartLinearVelocity = StartLinearVelocity;
			des.RemainStartAt = RemainStartAt;
			des.StartTime = StartTime;
			des.FullRemainTime = FullRemainTime;
			des.NoRemainFlag = NoRemainFlag;
			des.Stage = Stage;
		}
	}
}
