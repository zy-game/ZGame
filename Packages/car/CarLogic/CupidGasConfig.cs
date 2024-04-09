using UnityEngine;

namespace CarLogic
{
	public static class CupidGasConfig
	{
		private static readonly float StdFriction = 0.4f;

		public static float EngineForce = 1800f;

		public static Keyframe[] EngineCurveKeyframe = new Keyframe[8]
		{
			new Keyframe(0f, 1.8f),
			new Keyframe(0.2f, 1.8f),
			new Keyframe(0.5f, 1f),
			new Keyframe(0.6f, 0.7f),
			new Keyframe(0.7f, 0.6f),
			new Keyframe(0.8f, 0.9f),
			new Keyframe(0.9f, 0.6f),
			new Keyframe(1f, 0.3f)
		};

		public static AnimationCurve EngineCurve = new AnimationCurve(EngineCurveKeyframe);

		internal static float CalculateTopSpeed(CarView view)
		{
			float z = CarController.CalculateGroundFriction(view.carModel, StdFriction, Vector3.forward).z;
			float num = 0f;
			num += CarController.CalculateEngineForce(view.carModel, 1f, 0);
			num += CarController.CalculateEngineForce(EngineForce, EngineCurve, 1f);
			return Mathf.Sqrt(Mathf.Max(0f, (num - z) / (view.carModel.DragMultiplier.z * (float)view.carModel.AirFrictionFactor)));
		}
	}
}
