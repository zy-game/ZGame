using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CarState
	{
		public PlayerType CarPlayerType;

		public int GroundHit;

		public int WheelHitState;

		public AnimStage AnimationFlag;

		public Wheel[] Wheels;

		public WheelHit[] WheelHits;

		public HitWallDirection CollisionDirection;

		public bool HandBrake;

		public bool Drift;

		public bool N2Force;

		public N2ForceState N2State = new N2ForceState();

		public AirGroundState AirGround = new AirGroundState();

		public CollisionState CollisionState = new CollisionState();

		public float Throttle;

		public float Steer;

		public float HitDot;

		public bool HitLeft;

		public float HitTime;

		public float SteerChangeTime;

		public float LastRightDirTime;

		public float LinearVelocity;

		public SpecialType ApplyingSpecialType;

		public float[] HitLenghts;

		public Vector3 HitPoint;

		public Vector3 HitNormal;

		public Vector3 HitVelocity;

		public Vector3 ThrottleDirection;

		public Vector3 GroundNormal = Vector3.up;

		public Vector3 GroundForward;

		public Vector3 LastNormal = Vector3.up;

		public RacePathNode CurNode;

		public RacePathNode ClosestNode;

		public RacePathNode LastCorrectNode;

		public bool DirectionWrong = true;

		public bool Visible = true;

		public Int PastLapCount = 0;

		public Float PastDistance = 0;

		public int Rank;

		public Vector3 velocity;

		public Vector3 relativeVelocity;

		public Vector3 eularAngle;

		public Vector3 AirDrag;

		public Vector3 GroundFriction;

		public Vector3 AirDragMultiplier;

		public Vector3 ThrottleForce;

		public Vector3 Gravity;

		public DriftState CurDriftState = new DriftState();

		public Vector3 TotalForces;

		public CarCallBack CallBacks = new CarCallBack();

		public List<RaceItemBase> Items = new List<RaceItemBase>(2);

		public Queue<RaceItemBase> ItemEffectQueue = new Queue<RaceItemBase>(4);

		public LinkedList<RaceItemBase> ApplyItems = new LinkedList<RaceItemBase>();

		public LinkedList<PassiveTrigger> ApplyTriggers = new LinkedList<PassiveTrigger>();

		public List<CarSkillBase> Skills = new List<CarSkillBase>(4);

		[NonSerialized]
		public List<CarChipTechnologySkillBase> TechnologySkills = new List<CarChipTechnologySkillBase>();

		public float SpeedRatio;

		[NonSerialized]
		public Transform transform;

		[NonSerialized]
		public Rigidbody rigidbody;

		[NonSerialized]
		public CarView view;

		[HideInInspector]
		public ushort cloned;

		public int SidewayFrictionFlag;

		[HideInInspector]
		public ConfigurableJoint BalanceJoint;

		[NonSerialized]
		public byte TranslateInfoByte;

		[NonSerialized]
		public SpecialTriggerBase ApplyingSpecial;

		[NonSerialized]
		public int TransQTEok;

		[NonSerialized]
		public int TransQteAnimIndex = -1;

		[NonSerialized]
		internal bool reachedEnd;

		[NonSerialized]
		public Vector3 avgV = Vector3.zero;

		[NonSerialized]
		public bool Offline;

		[NonSerialized]
		public bool Clamping;

		internal Vector3 LastPosition;

		internal Vector3 LastVelocity;

		internal Quaternion LastRotation;

		private static Vector3 stdNormal = Vector3.up;

		public bool ReachedEnd
		{
			get
			{
				return reachedEnd;
			}
			set
			{
				reachedEnd = value;
				if (CallBacks != null && CallBacks.OnReachEnd != null)
				{
					CallBacks.OnReachEnd(reachedEnd);
				}
			}
		}

		public bool InTwistArea
		{
			get
			{
				if (transform == null)
				{
					return false;
				}
				if (LastCorrectNode == null)
				{
					return false;
				}
				return Vector3.Dot(LastCorrectNode.Up, stdNormal) < 0.96f;
			}
		}

		public void Copy(ref CarState des)
		{
			des.Steer = Steer;
			des.Throttle = Throttle;
			des.Drift = Drift;
			des.N2Force = N2Force;
			des.AirGround = AirGround;
			des.CollisionState = CollisionState;
			des.HitDot = HitDot;
			des.HitTime = HitTime;
			des.SteerChangeTime = SteerChangeTime;
			des.HitPoint = HitPoint;
			des.HitNormal = HitNormal;
			des.HitVelocity = HitVelocity;
			des.ThrottleDirection = ThrottleDirection;
			des.GroundNormal = GroundNormal;
			des.GroundForward = GroundForward;
			des.relativeVelocity = relativeVelocity;
			des.GroundHit = GroundHit;
			des.WheelHitState = WheelHitState;
			des.AnimationFlag = AnimationFlag;
			des.DirectionWrong = DirectionWrong;
			CurDriftState.CopyTo(des.CurDriftState);
			des.cloned++;
		}

		public void ClearState()
		{
			Steer = 0f;
			Throttle = 0f;
			Drift = false;
			N2Force = false;
		}
	}
}
