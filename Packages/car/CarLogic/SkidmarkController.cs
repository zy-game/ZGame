using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	internal class SkidmarkController : ControllerBase
	{
		internal class CustomHitInfo
		{
			private object _unityHit;

			private bool _isHit;

			public bool IsHit
			{
				get
				{
					return _isHit;
				}
				set
				{
					_isHit = value;
				}
			}

			public object UnityHit
			{
				get
				{
					return _unityHit;
				}
				set
				{
					_unityHit = value;
				}
			}

			public Vector3 point
			{
				get
				{
					if (_unityHit is RaycastHit)
					{
						return ((RaycastHit)_unityHit).point;
					}
					if (_unityHit is WheelHit)
					{
						return ((WheelHit)_unityHit).point;
					}
					return Vector3.zero;
				}
			}

			public Vector3 normal
			{
				get
				{
					if (_unityHit is RaycastHit)
					{
						return ((RaycastHit)_unityHit).normal;
					}
					if (_unityHit is WheelHit)
					{
						return ((WheelHit)_unityHit).normal;
					}
					return Vector3.up;
				}
			}

			public CustomHitInfo(object hit, bool isHit)
			{
				_unityHit = hit;
				_isHit = isHit;
			}
		}

		private Skidmarks[] skidmarks;

		private VerticalSkidmarks[] verticalmarks;

		private CarState carState;

		public float[] skidmarkTime;

		public float[] verskidmarkTime;

		private Vector3 vt;

		private Vector3 vt3;

		private Vector3 vt4;

		private Vector3 curPos;

		private Transform viewer;

		private bool inited;

		private EffectToggleBase[] sparks = new EffectToggleBase[2];

		private EffectToggleBase[] specialSparks;

		private Transform driftOffsetRoot;

		private bool _isRaycastSkid;

		private bool _useSkidmarkAnchor;

		private CustomHitInfo[] _rearWheelsHitInfo;

		public bool IsRaycastSkid
		{
			get
			{
				return _isRaycastSkid;
			}
			set
			{
				_isRaycastSkid = value;
				InitRearWheelsHitInfos();
			}
		}

		public EffectToggleBase[] Sparks
		{
			get
			{
				return sparks;
			}
			set
			{
				sparks = value;
			}
		}

		public EffectToggleBase[] SpecialSparks
		{
			get
			{
				return specialSparks;
			}
			set
			{
				specialSparks = value;
			}
		}

		public void SetSkidmarks(Skidmarks[] marks)
		{
			skidmarks = marks;
		}

		public void SetVerticalSkidmarks(VerticalSkidmarks[] marks)
		{
			verticalmarks = marks;
		}

		public void Init(AbstractView view, CarState state)
		{
			carState = state;
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			CarView view = carState.view;
			view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
			if (active)
			{
				CarView view2 = carState.view;
				view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(update));
				SetUpSkidmarks();
			}
		}

		private void update()
		{
			if (base.Active)
			{
				UpdateSkidmarks(carState.relativeVelocity);
			}
		}

		public void ResetAllSkidmarks()
		{
			if (verticalmarks != null)
			{
				for (int i = 0; i < verticalmarks.Length; i++)
				{
					verticalmarks[i].ResetAllSkidmarks();
				}
			}
		}

		private void SetUpSkidmarks()
		{
			if (!inited)
			{
				inited = true;
				viewer = Camera.main.transform;
				loadSparks();
				skidmarkTime = new float[4];
				verskidmarkTime = new float[4];
				for (int i = 0; i < 4; i++)
				{
					skidmarkTime[i] = 0f;
					verskidmarkTime[i] = 0f;
				}
			}
		}

		private void loadSparks()
		{
			ResCallback cb = delegate(UnityEngine.Object o)
			{
				if (carState != null && !(carState.view == null))
				{
					EffectAnchor[] wheelEffectAnchors = carState.view.WheelEffectAnchors;
					if (wheelEffectAnchors.Length != 0)
					{
						GameObject gameObject = new GameObject();
						gameObject.name = "WheelEffect";
						gameObject.transform.parent = carState.view.transform;
						gameObject.transform.localScale = Vector3.one;
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localRotation = Quaternion.identity;
						for (int i = 0; i < wheelEffectAnchors.Length; i++)
						{
							GameObject go = UnityEngine.Object.Instantiate(o) as GameObject;
							Transform transform = go.transform;
							transform.parent = gameObject.transform;
							transform.localPosition = wheelEffectAnchors[i].Position;
							transform.localRotation = wheelEffectAnchors[i].Rotation;
							sparks[i] = go.AddComponent<DistanceParticleEffectToggle>();
							sparks[i].Init();
							sparks[i].IgnoreRenders = true;
							sparks[i].Active = false;
							CarView view = carState.view;
							view.OnRelease = (Action)Delegate.Combine(view.OnRelease, (Action)delegate
							{
								if (go != null)
								{
									UnityEngine.Object.Destroy(go);
								}
							});
						}
					}
				}
			};
			RacePathManager.LoadDependence(RacePathManager.ActiveInstance, 2, cb, "Effects/Sence/", RaceConfig.SkidSparkEffect);
		}

		private void SetSparksActive()
		{
			for (int i = 0; i < sparks.Length; i++)
			{
				if (sparks[i] != null)
				{
					int num = i + carState.view.frontWheels.Length;
					bool flag = carState.CurDriftState.Stage != 0 && carState.Wheels[num].collider.isGrounded;
					if (flag != sparks[i].Active)
					{
						sparks[i].Active = flag;
					}
				}
			}
		}

		private void SetVerticalSkidmark(int wheelCount, CustomHitInfo wh, Wheel w)
		{
			if (verticalmarks == null)
			{
				return;
			}
			VerticalSkidmarks verticalSkidmarks = verticalmarks[wheelCount % verticalmarks.Length];
			if (!verticalSkidmarks)
			{
				return;
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			if (carState.CurDriftState.Stage != 0)
			{
				curPos = wh.point + vt;
				int lastVerticalmark = w.lastVerticalmark;
				if (lastVerticalmark < 0)
				{
					w.lastVerticalmark = verticalSkidmarks.AddSkidMark(curPos, wh.normal, 1f, -1, wheelCount);
					return;
				}
				VerticalSkidmarks.VerticalMark verticalMark = verticalSkidmarks.skidmarks[lastVerticalmark % verticalSkidmarks.maxMarks];
				w.lastVerticalmark = verticalSkidmarks.AddSkidMark(curPos, wh.normal, 1f, w.lastVerticalmark, wheelCount);
			}
			else
			{
				w.lastVerticalmark = -1;
			}
		}

		private void SetFlatSkidmark(int wheelCount, Vector3 relativeVelocity, CustomHitInfo wh, Wheel w)
		{
			if (this.skidmarks == null)
			{
				return;
			}
			Skidmarks skidmarks = this.skidmarks[wheelCount % this.skidmarks.Length];
			if (!(skidmarks != null))
			{
				return;
			}
			float num = ((_isRaycastSkid || _useSkidmarkAnchor) ? 0f : (Time.fixedDeltaTime * 2f));
			if (carState.CurDriftState.Stage != 0)
			{
				float markWidth = skidmarks.markWidth;
				float intensity = Mathf.Clamp01(Mathf.Lerp(0f, 1f, Mathf.Abs(relativeVelocity.x / 16f)));
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF && carState.ApplyingSpecialType != SpecialType.Translate)
				{
					vt = carState.rigidbody.velocity * num;
				}
				else
				{
					vt = carState.velocity * num;
				}
				SpecialType applyingSpecialType = carState.ApplyingSpecialType;
				int num2 = 1;
				vt3 = wh.point + vt;
				int lastSkidmark = w.lastSkidmark;
				if (lastSkidmark <= -1)
				{
					w.lastSkidmark = skidmarks.AddSkidMark(vt3, wh.normal, intensity, lastSkidmark);
					return;
				}
				Skidmarks.MarkSection markSection = skidmarks.skidmarks[lastSkidmark % skidmarks.maxMarks];
				w.lastSkidmark = skidmarks.AddSkidMark(vt3, wh.normal, intensity, w.lastSkidmark);
			}
			else
			{
				int lastSkidmark2 = w.lastSkidmark;
				int num3 = 0;
				w.lastSkidmark = -1;
			}
		}

		private void InitRearWheelsHitInfos()
		{
			int num = ((carState.view.SkidmarkAnchors != null && carState.view.SkidmarkAnchors.Length >= 2) ? carState.view.SkidmarkAnchors.Length : 0);
			_rearWheelsHitInfo = new CustomHitInfo[2];
			for (int i = 0; i < 2; i++)
			{
				int num2 = carState.Wheels.Length - 1 - i;
				if (num > 0 || _isRaycastSkid)
				{
					_rearWheelsHitInfo[i] = new CustomHitInfo(default(RaycastHit), isHit: false);
				}
				else
				{
					_rearWheelsHitInfo[i] = new CustomHitInfo(carState.WheelHits[num2], (carState.GroundHit & (1 << num2)) != 0);
				}
			}
		}

		private void UpdateSkidmarks(Vector3 relativeVelocity)
		{
			if (carState.WheelHits == null)
			{
				carState.WheelHits = new WheelHit[carState.Wheels.Length];
			}
			if (carState.CurDriftState.Stage != 0)
			{
				vt4 = -carState.transform.forward * Mathf.Lerp(0.05f, 0.25f, Mathf.Abs(relativeVelocity.x) / 30f);
				vt4.y = 0f;
			}
			int num = carState.Wheels.Length;
			int num2 = ((carState.view.SkidmarkAnchors != null && carState.view.SkidmarkAnchors.Length >= 2) ? carState.view.SkidmarkAnchors.Length : 0);
			_useSkidmarkAnchor = num2 > 0;
			if (_rearWheelsHitInfo == null || _rearWheelsHitInfo.Length != 2)
			{
				InitRearWheelsHitInfos();
			}
			for (int i = 0; i < 2; i++)
			{
				int num3 = num - 1 - i;
				Wheel wheel = carState.Wheels[num3];
				WheelCollider collider = wheel.collider;
				if (num2 > 0)
				{
					RaycastHit hitInfo = (RaycastHit)_rearWheelsHitInfo[i].UnityHit;
					EffectAnchor effectAnchor = carState.view.SkidmarkAnchors[num2 - 1 - i];
					_rearWheelsHitInfo[i].IsHit = Physics.Raycast(carState.view.transform.TransformPoint(effectAnchor.Position) + collider.transform.up * 0.5f, -collider.transform.up, out hitInfo, 1f, LayerMask.GetMask("Road"));
					_rearWheelsHitInfo[i].UnityHit = hitInfo;
				}
				else if (_isRaycastSkid)
				{
					RaycastHit hitInfo2 = (RaycastHit)_rearWheelsHitInfo[i].UnityHit;
					_rearWheelsHitInfo[i].IsHit = Physics.Raycast(collider.transform.position + collider.transform.up * 0.5f, -collider.transform.up, out hitInfo2, 1f, LayerMask.GetMask("Road"));
					_rearWheelsHitInfo[i].UnityHit = hitInfo2;
				}
				else
				{
					carState.Wheels[num3].collider.GetGroundHit(out carState.WheelHits[num3]);
					_rearWheelsHitInfo[i].UnityHit = carState.WheelHits[num3];
					_rearWheelsHitInfo[i].IsHit = (carState.GroundHit & (1 << num3)) != 0;
				}
				if (_rearWheelsHitInfo[i].IsHit)
				{
					SetFlatSkidmark(num3, relativeVelocity, _rearWheelsHitInfo[i], wheel);
					SetVerticalSkidmark(num3, _rearWheelsHitInfo[i], wheel);
					continue;
				}
				if (skidmarks != null)
				{
					wheel.lastSkidmark = -1;
				}
				if (verticalmarks != null)
				{
					wheel.lastVerticalmark = -1;
				}
			}
			SetSparksActive();
		}
	}
}
