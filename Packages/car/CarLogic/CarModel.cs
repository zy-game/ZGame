using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CarModel
	{
		public CarModelType carModelType;

		[SerializeField]
		protected bool airShip;

		public EffectGroup AirShipEffectGroup;

		public const float MinDrag = 0f;

		public Float CarWeight = 1500;

		public Float Gravity = 30;

		public float ExtraGravity = 4f;

		public Vector3 DragMultiplier = new Vector3(12f, 1f, 1f);

		public Float[] EngineForces = new Float[4] { 1600, 600, 600, 600 };

		public Float GroundFrictionFactor = 200;

		public Float GroundFrictionScale = 1;

		public Float AirFrictionFactor = 50;

		public float SideWayFriction = 3f;

		public Float N2DownFactor = 1200;

		public Float SmallN2DownFactor = 500;

		public Float[] MaxSpeeds;

		public Float[] MaxSpeedsKmph;

		public Float N2ForceTime = 5;

		public Float SmallN2ForceTime = 0.6f;

		public Float SmallN2ForceTime2 = 0.6f;

		public Float SECOND_SMALL_GAS_WAIT_TIME = 1;

		public int NumberOfGears = 3;

		public Float AngularDrag = 8;

		public Float MaximumTurn = 90;

		public Float MinimumTurn = 60;

		public Float MinRoateRotio = 0.5f;

		public Float FullRotateTime = 2.5f;

		public Float SteerSpeed = 5;

		public Float MinRatioSteerDragOn = 0.45f;

		public AnimationCurve SteerCurve = AnimationCurve.EaseInOut(-1f, 0f, 1f, 0f);

		public AnimationCurve SteerDragCurve = AnimationCurve.Linear(0f, 0.05f, 2.5f, 0.3f);

		public AnimationCurve[] EngineCurves;

		public float MinAngularSpeed;

		public float MinDriftableSpeedRatio = 0.2f;

		[HideInInspector]
		public float MinDriftableSpeed = 100f;

		public Float MinDriftRadius = 4;

		public Float PerfectDriftTime = 1f;

		public float Bouncenese = 0.2f;

		public float BackwardFactor = 1f;

		public Float suspensionRange = 1.27f;

		public Float suspensionDamper = 200;

		public Float suspensionSpringFront = 8000;

		public Float suspensionSpringRear = 8000;

		public Vector3 centerOfMass;

		[HideInInspector]
		public Float FrontWheelRadius = 0.2f;

		[HideInInspector]
		public Float RearWheelRadius = 0.2f;

		public Float GasFactor = 0.5f;

		public AnimationCurve GasFactorDfsCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

		public AnimationCurve GasFactorDmvCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

		public Float DfsAdjusted = 0f;

		public Float DmvAdjusted = 0f;

		private WheelFrictionCurve wfc;

		public Float DeltaAcceleration = 0f;

		public Float Rotate0To90Time = 1f;

		public Float DriftRotateAngle = 0f;

		public Float TurnRotateAngle = 0f;

		[NonSerialized]
		[HideInInspector]
		public float RemainFixtime;

		[NonSerialized]
		public float MaxLinearVelocity;

		public bool AirShip
		{
			get
			{
				return airShip;
			}
			set
			{
				airShip = value;
			}
		}

		public void Set(CarModel other)
		{
			AngularDrag = other.AngularDrag;
			Bouncenese = other.Bouncenese;
			CarWeight = other.CarWeight;
			centerOfMass = other.centerOfMass;
			DragMultiplier = other.DragMultiplier;
			MaxSpeeds = other.MaxSpeeds;
			MaxSpeedsKmph = other.MaxSpeedsKmph;
			EngineForces = other.EngineForces;
			EngineCurves = other.EngineCurves;
			MaximumTurn = other.MaximumTurn;
			MinDriftableSpeed = other.MinDriftableSpeed;
			MinDriftableSpeedRatio = other.MinDriftableSpeedRatio;
			MinimumTurn = other.MinimumTurn;
			NumberOfGears = other.NumberOfGears;
			suspensionDamper = other.suspensionDamper;
			suspensionRange = other.suspensionRange;
			suspensionSpringFront = other.suspensionSpringFront;
			suspensionSpringRear = other.suspensionSpringRear;
			wfc = other.wfc;
			MinDriftRadius = other.MinDriftRadius;
			PerfectDriftTime = other.PerfectDriftTime;
			DeltaAcceleration = other.DeltaAcceleration;
			GasFactorDfsCurve = other.GasFactorDfsCurve;
			GasFactorDmvCurve = other.GasFactorDmvCurve;
			GroundFrictionFactor = other.GroundFrictionFactor;
			GroundFrictionScale = other.GroundFrictionScale;
			SmallN2DownFactor = other.SmallN2DownFactor;
			N2DownFactor = other.N2DownFactor;
			Rotate0To90Time = other.Rotate0To90Time;
			DriftRotateAngle = other.DriftRotateAngle;
			TurnRotateAngle = other.TurnRotateAngle;
			RemainFixtime = other.RemainFixtime;
			carModelType = other.carModelType;
		}
	}
}
