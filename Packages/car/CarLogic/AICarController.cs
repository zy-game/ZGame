using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class AICarController : ControllerBase
	{
		public MonoBehaviour RecorderLoaderMono;

		public const int FLAG_LAP_COUNT = 0;

		public const int FLAG_SMALL_GAS = 2;

		public const int FLAG_BIG_GAS = 1;

		public const int FLAG_DOUBLE_GAS = 3;

		public const int FLAG_DRIFT = 4;

		public const int FLAG_STEER_LEFT = 5;

		public const int FLAG_STEER_RIGHT = 6;

		public const int FLAG_THROTTLE_BACK = 7;

		public bool recording;

		private bool jointMove = true;

		private CarView carView;

		protected CarState carState;

		private GameObject gameObject;

		private CharacterController controller;

		public CarRecor Recorder = new CarRecor();

		private float recordInterval = 0.1f;

		[SerializeField]
		private int lastFlag;

		private int lastLap;

		private float lastTime;

		private float moveStartTime;

		private float lostMoveTime;

		private float itemDelayStartAt;

		private Vector3 lastPos;

		private Vector3 vt1 = Vector3.zero;

		private Vector3 vt2 = Vector3.zero;

		private Vector3 vt3 = Vector3.zero;

		private Quaternion qt1;

		private Quaternion qt2;

		private Quaternion lastRot;

		public List<CarPoint> pathList;

		private LinkedList<ItemToggle> itList = new LinkedList<ItemToggle>();

		private LinkedList<QTEToggle> qteList = new LinkedList<QTEToggle>();

		private LinkedList<GasToggle> gasList = new LinkedList<GasToggle>();

		private CarPoint lastTarget;

		private CarPoint target;

		private int index;

		private DriftStage lastDriftStage;

		private int lastN2Level;

		[SerializeField]
		private float timeScale = 1f;

		private bool _paused = true;

		[SerializeField]
		private bool paused = true;

		private float crashScale = 0.75f;

		private float _currentTimeScale = 1f;

		public bool AutoStartMove;

		private float autoStartTime = 3f;

		public Action<CarState, ItemToggle> OnPassItemPoint;

		private Dictionary<object, float> applyScale = new Dictionary<object, float>(4);

		private TeamGasScraper teamGas;

		private float curRatio;

		private Queue<float> itScaleQueue = new Queue<float>(100);

		private HashSet<object> itScaleFlag = new HashSet<object>();

		private Rigidbody jRigidbody;

		private SpringJoint joint;

		private Action<CarView> OnEndAIMove;

		private float pauseAt;

		private Vector3 pauseVel;

		private TweenerCore<float, float, FloatOptions> timeScaleTweener;

		private float continueRotTime;

		private HitWallDirection lastCollisionDirection;

		private float sideWayFriction = 50f;

		private bool closedLoop;

		public bool JointMove
		{
			get
			{
				return jointMove;
			}
			set
			{
				jointMove = value;
			}
		}

		public float RecordInterval
		{
			get
			{
				return recordInterval;
			}
			set
			{
				recordInterval = value;
			}
		}

		public float SpeedTimeScale
		{
			get
			{
				return timeScale;
			}
			set
			{
				timeScale = value;
				_currentTimeScale = timeScale;
			}
		}

		public float MoveStartTime => moveStartTime;

		internal int MapId => Recorder.MapId;

		internal int TrackId => Recorder.TrackId;

		internal uint ItemStrategy => Recorder.ItemStrategy;

		internal List<ItemToggle> Toggles => Recorder.itemToggles;

		internal TeamGasScraper TeamGas => teamGas;

		internal int RecordCount
		{
			get
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					return index;
				}
				if (carState.CarPlayerType == PlayerType.PALYER_AI && pathList != null)
				{
					return pathList.Count;
				}
				return 0;
			}
		}

		public GasToggle NextGasPoint
		{
			get
			{
				if (gasList == null || gasList.First == null)
				{
					return null;
				}
				return gasList.First.Value;
			}
		}

		internal bool Paused
		{
			get
			{
				return paused;
			}
			set
			{
				if ((bool)joint && paused != value)
				{
					if (value)
					{
						pauseAt = Time.time;
						pauseVel = carState.velocity;
						carState.velocity.Set(0f, 0f, 0f);
						carState.relativeVelocity.Set(0f, 0f, 0f);
						breakJoint();
					}
					else
					{
						moveStartTime += Time.time - pauseAt;
						lastTime += Time.time - pauseAt;
						carState.velocity = pauseVel;
						carState.relativeVelocity = carState.transform.InverseTransformDirection(pauseVel);
						resetJoint();
					}
				}
				paused = value;
			}
		}

		public float PastTime
		{
			get
			{
				if (!paused)
				{
					return Time.time - moveStartTime;
				}
				return 0f;
			}
		}

		public void SetCarCecor(CarRecor recor)
		{
			Recorder = recor;
		}

		public void Init(CarView view, CarState state, float timeScale = 1f)
		{
			this.timeScale = timeScale;
			carView = view;
			carState = state;
			gameObject = view.gameObject;
			_currentTimeScale = timeScale;
		}

		public override void OnActiveChange(bool active)
		{
			if (this.carView == null || carState == null || carState.view == null)
			{
				return;
			}
			if (active)
			{
				CarView carView = this.carView;
				carView.OnFixedupdate = (Action)Delegate.Combine(carView.OnFixedupdate, new Action(onFixedUpdate));
				controller = this.carView.GetComponent<CharacterController>();
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF || carState.CarPlayerType != PlayerType.PALYER_AI)
				{
					return;
				}
				CarView view = carState.view;
				view.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
				if ((bool)controller)
				{
					controller.enabled = false;
				}
				if (JointMove)
				{
					resetJoint();
				}
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnLapFinish = (Action<CarState>)Delegate.Combine(callBacks.OnLapFinish, new Action<CarState>(onLapFinish));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				CarView view2 = carState.view;
				view2.OnVisibleChange = (Action<bool>)Delegate.Combine(view2.OnVisibleChange, new Action<bool>(onVisibleChange));
				Collider[] bodyColliders = carState.view.BodyColliders;
				if (bodyColliders == null || bodyColliders.Length == 0)
				{
					return;
				}
				for (int i = 0; i < bodyColliders.Length; i++)
				{
					if (bodyColliders[i] != null)
					{
						PhysicMaterial material = bodyColliders[i].material;
						if (material != null)
						{
							material.bounciness = 0f;
							material.dynamicFriction = 0f;
							material.staticFriction = 0f;
							material.bounceCombine = PhysicMaterialCombine.Minimum;
						}
					}
				}
			}
			else
			{
				CarView carView2 = this.carView;
				carView2.OnFixedupdate = (Action)Delegate.Remove(carView2.OnFixedupdate, new Action(onFixedUpdate));
				CarView carView3 = this.carView;
				carView3.OnUpdate = (Action)Delegate.Remove(carView3.OnUpdate, new Action(onFixedUpdate));
				CarCallBack callBacks3 = carState.CallBacks;
				callBacks3.OnLapFinish = (Action<CarState>)Delegate.Remove(callBacks3.OnLapFinish, new Action<CarState>(onLapFinish));
				CarCallBack callBacks4 = carState.CallBacks;
				callBacks4.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks4.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				CarView view3 = carState.view;
				view3.OnVisibleChange = (Action<bool>)Delegate.Remove(view3.OnVisibleChange, new Action<bool>(onVisibleChange));
				CarView view4 = carState.view;
				view4.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view4.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
				if (JointMove)
				{
					breakJoint();
				}
			}
		}

		protected virtual void onFixedUpdate()
		{
			if (!base.Active)
			{
				return;
			}
			switch (carState.CarPlayerType)
			{
				case PlayerType.PALYER_AI:
					replay();
					break;
				case PlayerType.PLAYER_SELF:
					if (recording)
					{
						recordUpdate();
					}
					else
					{
						replay();
					}
					break;
			}
		}

		internal void StartAiRecord()
		{
			lastTime = Time.time;
			moveStartTime = lastTime;
			index = 0;
			paused = false;
			record();
		}

		internal void StopRecord()
		{
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				paused = true;
			}
		}

		public void SaveRecord()
		{
			if (!paused)
			{
				record();
			}
			StopRecord();
			Recorder.Save();
			LogWarning("Save Record at ", Time.time);
			Recorder.OnDestroy();
		}

		private IEnumerator load(string filename, Action<byte[]> callback)
		{
			WWW www = null;
			try
			{
				www = new WWW(filename);
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Debug.LogWarning(e.ToString());
			}
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError("CarRecor.load(string filename:\t" + www.error);
			}
			if (callback != null && www != null)
			{
				callback(www.bytes);
			}
			if (www != null && null != www.assetBundle)
			{
				www.assetBundle.Unload(unloadAllLoadedObjects: true);
			}
		}

		public void LoadRecord(string path, float aiSpeed = 1f, Action callback = null)
		{
			Debug.Log("AiPath:\t" + path);
			Recorder.FilePath = path;
			SpeedTimeScale = aiSpeed;
			Action<byte[]> callback2 = delegate(byte[] data)
			{
				switch (CarRecor.ParseVersion(data))
				{
					case 2:
					case 4:
						Recorder = new CarShadowRecor();
						break;
					default:
						Debug.LogError("未定义的录像版本号");
						break;
					case 1:
					case 3:
					case 6:
						break;
				}
				if (Recorder != null)
				{
					Recorder.FilePath = path;
					Recorder.Load(data);
					pathList = Recorder.trail;
					itList = new LinkedList<ItemToggle>(Recorder.itemToggles);
					qteList = new LinkedList<QTEToggle>(Recorder.qteToggles);
					gasList = new LinkedList<GasToggle>(Recorder.gasToggles);
					FillItemPointPosition(itList, pathList);
					if (AutoStartMove)
					{
						StartAiMove();
					}
					if (callback != null)
					{
						callback();
					}
					Log("Load Path from file: ", Recorder.ToString());
				}
			};
			if (null != RecorderLoaderMono && RecorderLoaderMono.isActiveAndEnabled)
			{
				Log("Load AI From RecorderLoaderMono");
				RecorderLoaderMono.StartCoroutine(load(path, callback2));
			}
			else
			{
				Log("Load AI From CarView");
				carView.StartCoroutine(load(path, callback2));
			}
		}

		public void AddItemPoint(float time, byte index = 0, RaceItemId itemId = RaceItemId.NONE)
		{
			Recorder.AddItemPoint(time, index, (ushort)itemId);
			if (!paused && carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				record();
			}
		}

		public void AddQtePoint(float time, byte flag)
		{
			Recorder.AddQtePoint(time, flag);
			if (!paused && carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				record();
			}
		}

		public void AddGasPoint(float time, float percent)
		{
			Recorder.AddGasPoint(time, percent);
			if (!paused && carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				record();
			}
		}

		internal ItemToggle GetTogglePoint(int pointIndex)
		{
			if (Recorder == null)
			{
				LogWarning("Null recorder.");
				return null;
			}
			if (pointIndex < 0 || pointIndex >= Recorder.itemToggles.Count)
			{
				LogWarning("Error point index : ", pointIndex, " max=", Recorder.itemToggles.Count);
				return null;
			}
			return Recorder.itemToggles[pointIndex];
		}

		public static void FillItemPointPosition(LinkedList<ItemToggle> list, List<CarPoint> plist)
		{
			if (list == null || list.First == null)
			{
				return;
			}
			float num = 0f;
			LinkedListNode<ItemToggle> first = list.First;
			float time = first.Value.time;
			for (int i = 0; i < plist.Count; i++)
			{
				num += plist[i].DeltaTime;
				if (num > time && i > 0)
				{
					CarPoint carPoint = plist[i - 1];
					first.Value.pos = Vector3.Lerp(plist[i].pos.V3, carPoint.pos.V3, (num - time) / plist[i].DeltaTime);
					first = first.Next;
					if (first == null)
					{
						break;
					}
					time = first.Value.time;
				}
			}
		}

		protected virtual void recordUpdate()
		{
			if (paused)
			{
				return;
			}
			if (lastLap != carState.PastLapCount)
			{
				record();
				lastLap = carState.PastLapCount;
				return;
			}
			float num = recordInterval;
			vt1 = carState.rigidbody.angularVelocity;
			if (Mathf.Abs(vt1.y) < 0.05f && Mathf.Abs(vt1.x) < 0.05f && Mathf.Abs(vt1.z) < 0.05f && (carState.transform.position - lastPos).sqrMagnitude < 1500f)
			{
				num *= 50f;
			}
			if (Time.time - lastTime > num)
			{
				record();
			}
		}

		public void RecordImmediate()
		{
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF && !paused)
			{
				record();
			}
		}

		protected virtual void record()
		{
			Recorder.AddPositionPoint(gameObject, Time.time - lastTime, getStateFlag(carState), carState.view.GetCarModelRotateZ());
			lastPos = carState.transform.position;
			lastTime = Time.time;
			index++;
		}

		private int getStateFlag(CarState state)
		{
			int num = (int)state.PastLapCount & 7;
			if (state.N2State.Level != 0)
			{
				num |= 1 << state.N2State.Level;
			}
			if (state.AirGround.DoingGas)
			{
				num |= 4;
			}
			if (state.CurDriftState.Stage != 0)
			{
				num |= 0x10;
			}
			if (state.Steer < 0f)
			{
				num |= 0x20;
			}
			else if (state.Steer > 0f)
			{
				num |= 0x40;
			}
			if (state.Throttle < 0f)
			{
				num |= 0x80;
			}
			return num;
		}

		protected virtual void onTriggerSpecial(Collider c)
		{
			if (carState.CarPlayerType == PlayerType.PALYER_AI)
			{
				switch (c.gameObject.layer)
				{
					case 20:
						onToggleObstacle(c);
						break;
					case 18:
						onToggleSpeedUp(c);
						break;
					case 19:
						onToggleTranslation(c);
						break;
				}
			}
		}

		protected virtual void onToggleObstacle(Collider c)
		{
		}

		protected virtual void onToggleSpeedUp(Collider c)
		{
		}

		protected virtual void onToggleTranslation(Collider c)
		{
			TranslateStarter component = c.gameObject.GetComponent<TranslateStarter>();
			if (!(component == null))
			{
				TranslatePath path = null;
				int pathCloest = component.GetPathCloest(carState, ref path);
				TranslateTrigger translateTrigger = new TranslateTrigger(path, path.TotalLength / (float)RaceConfig.TranslateSpeed * component.TimeScale, component.EndTrigger, component.PushGrounded, component.TimeCurve);
				translateTrigger.Toggle(carState);
			}
		}

		private void buildTargetRigidbody()
		{
			Debug.LogError("Modify by Sbin: buildTargetRigidbody in AiCarController , Not use jointmove");
			JointMove = false;
			RigidbodyTool.SetConstraints(carState.view, RigidbodyConstraints.FreezeRotation);
			carState.rigidbody.drag = 9f;
			carState.rigidbody.mass = (float)carState.view.carModel.CarWeight * 0.8f;
			carState.view.SetSideWayFriction(sideWayFriction);
			CarView view = carState.view;
			view.OnRelease = (Action)Delegate.Combine(view.OnRelease, (Action)delegate
			{
				if ((bool)jRigidbody)
				{
					UnityEngine.Object.Destroy(jRigidbody.gameObject);
				}
			});
		}

		public void breakJoint()
		{
			Debug.Log("breakJoint");
			if ((bool)joint)
			{
				joint.connectedBody = null;
			}
		}

		public void resetJoint()
		{
			Debug.Log("resetJoint");
			if (!joint)
			{
				buildTargetRigidbody();
			}
			if ((bool)joint && (bool)carState.rigidbody)
			{
				Debug.Log(" --- AICarController in CarLogic.Dll ---  Modify by LiuShibin");
				joint.GetComponent<Rigidbody>().position = carState.rigidbody.position;
				joint.transform.position = carState.rigidbody.position;
				joint.connectedBody = carState.rigidbody;
			}
		}

		protected virtual void replay()
		{
			if (paused)
			{
				return;
			}
			if (pathList == null || index >= pathList.Count)
			{
				LogWarning("Path list error");
				return;
			}
			if (pathList.Count < 2)
			{
				LogWarning("Path list to small.");
				return;
			}
			if (isBeyong(Time.time - lastTime))
			{
				next();
			}
			if (!paused && Time.time - moveStartTime > (float)carState.view.carModel.SmallN2ForceTime && (double)(Time.time - moveStartTime) < (double)(float)carState.view.carModel.SmallN2ForceTime + 0.1)
			{
				lastFlag = 0;
			}
			if (base.Active && target != null && target.DeltaTime != 0f)
			{
				if (carView.RunState == RunState.Crash && carView.LastRunState == RunState.Run)
				{
					float crashTimeScale = timeScale * 0.9f;
					if (timeScaleTweener != null)
					{
						DOTween.Kill(timeScaleTweener);
					}
					timeScaleTweener = DOTween.To(() => crashTimeScale, delegate(float scale)
					{
						timeScale = scale;
					}, _currentTimeScale, 3f);
				}
				if (timeScale < _currentTimeScale)
				{
					lostMoveTime += Time.deltaTime * (_currentTimeScale - timeScale);
					moveStartTime += Time.deltaTime * (_currentTimeScale - timeScale);
				}
				else if (moveStartTime > 0f)
				{
					float num = Mathf.Min(lostMoveTime, Time.deltaTime * (_currentTimeScale - timeScale));
					lostMoveTime -= num;
					moveStartTime -= num;
				}
				if (JointMove && (bool)joint && joint.connectedBody == null)
				{
					joint.connectedBody = carState.rigidbody;
				}
				updateCurRatio();
				emulate();
				if (lastDriftStage != carState.CurDriftState.Stage)
				{
					onDriftStateChange(lastDriftStage);
				}
				if (lastN2Level != carState.N2State.Level)
				{
					onN2StateChange(lastN2Level);
				}
				if (carState.CurDriftState.Stage != 0)
				{
					if (carState.CallBacks.OnDrift != null)
					{
						carState.CallBacks.OnDrift(CarEventState.EVENT_DOING);
					}
					if (carState.CallBacks.OnAIDrift != null)
					{
						carState.CallBacks.OnAIDrift(carState, CarEventState.EVENT_DOING);
					}
				}
			}
			lastDriftStage = carState.CurDriftState.Stage;
			lastN2Level = carState.N2State.Level;
		}

		private void updateCurRatio()
		{
			if (target != null)
			{
				float num = timeScale;
				if (Time.time - moveStartTime <= autoStartTime)
				{
					num = Mathf.Clamp01((Time.time - moveStartTime) / Mathf.Max(autoStartTime, 0.1f));
					num *= timeScale;
				}
				curRatio = num * (Time.time - lastTime) / target.DeltaTime;
				curRatio = Mathf.Clamp01(curRatio);
			}
		}

		private void onVisibleChange(bool visible)
		{
			if (carState.CarPlayerType == PlayerType.PALYER_AI)
			{
				updateCurRatio();
				emulate(forceRot: true);
			}
		}

		private void emulate(bool forceRot = false)
		{
			if (itScaleQueue.Count != 0)
			{
				float num = itScaleQueue.Dequeue();
				AddSpeedScale(1f / num);
			}
			if (carState.Visible || forceRot)
			{
				rotate(curRatio);
			}
			setState(lastFlag);
			move(curRatio);
		}

		public void AiMoveTime(float startTime, bool setPos = true, Action<CarView> endCallback = null)
		{
			AiMove(startTime, -1, setPos, endCallback);
		}

		public void AiMoveNearly(Vector3 carPt, float startSpeed, Action<CarView> endCallback = null)
		{
			sideWayFriction = 0f;
			float num = 0f;
			float num2 = 0f;
			List<CarPoint> list = pathList;
			if (pathList == null || pathList.Count == 0)
			{
				Debug.LogError("赛道路径信息为空, 无能为力~, path=" + Recorder.FilePath);
				return;
			}
			if (pathList[0].Lap == 1)
			{
				list = Underline.Filter(pathList, (CarPoint point) => point.Lap == 1);
			}
			else
			{
				Debug.LogError("Lap信息没有生成, 请检查是否调用了Ai.Recorder.MarkLapNum");
			}
			int startIndex = FindNearPointIndexNew(carPt, list);
			CarPoint carPoint = list[startIndex];
			if (carPt != Vector3.zero)
			{
				carView.transform.position = carPoint.pos.V3;
				carView.transform.rotation = carPoint.rot.Q;
			}
			if (Recorder.RealSpeed >= 0f)
			{
				carView.GetComponent<Rigidbody>().velocity = carView.transform.forward * Recorder.RealSpeed;
			}
			if (Application.isEditor)
			{
				GameObject gameObject = GameObject.Find("花车传入点") ?? new GameObject("花车传入点");
				gameObject.transform.position = carPt;
				GameObject gameObject2 = GameObject.Find("花车") ?? new GameObject("花车");
				gameObject2.transform.position = carView.transform.position;
				gameObject2.transform.rotation = carView.transform.rotation;
			}
			AiMoveIndex(startIndex, setPos: false);
		}

		public void AiMoveIndex(int startIndex, bool setPos = true, Action<CarView> endCallback = null)
		{
			AiMove(0f, startIndex, setPos, endCallback);
		}

		private int FindNearPointIndexNew(Vector3 carPt, List<CarPoint> allPoints)
		{
			CarPoint carPoint = null;
			float num = 0f;
			for (int i = 0; i < allPoints.Count; i++)
			{
				if (i == 0)
				{
					carPoint = allPoints[i];
					num = (carPt - carPoint.pos.V3).magnitude;
					continue;
				}
				float magnitude = (carPt - allPoints[i].pos.V3).magnitude;
				if (magnitude < num)
				{
					carPoint = allPoints[i];
					num = magnitude;
				}
			}
			if (carPoint != null)
			{
				int num2 = Math.Max(0, carPoint.index - 1);
				if (Recorder.TotalLap == 1)
				{
					float num3 = Vector3.Distance(allPoints[0].pos.V3, allPoints[num2].pos.V3);
					if (num3 < 20f && num2 > allPoints.Count / 2)
					{
						float num4 = Vector3.Distance(allPoints[0].pos.V3, carPt);
						for (int j = 1; j < allPoints.Count; j++)
						{
							float num5 = Vector3.Distance(allPoints[j].pos.V3, carPt);
							if (num5 <= num4)
							{
								num4 = num5;
								continue;
							}
							num2 = j - 1;
							break;
						}
					}
				}
				Debug.Log($"FindNearPointIndexNew 找到坐标点序号: {num2}");
				return num2;
			}
			Debug.LogError("FindNearPointIndexNew 算法有问题?? 没有找到任何附近的坐标点");
			return 0;
		}

		private int FindNearPointIndex(Vector3 carPt, List<CarPoint> allPoints)
		{
			int num = 5;
			List<CarPoint> list = FastFindRangePoints(carPt, num, allPoints);
			if (list.Count == 0)
			{
				Debug.Log("扩大搜索范围");
				while (list.Count == 0)
				{
					list = FastFindRangePoints(carPt, num *= 2, allPoints);
				}
			}
			else
			{
				Debug.Log("缩小搜索范围");
				while (list.Count > 5 && num != 0)
				{
					List<CarPoint> list2 = FastFindRangePoints(carPt, num /= 2, list);
					if (list2.Count == 0)
					{
						break;
					}
					list = list2;
				}
			}
			if (list.Count == 0)
			{
				Debug.LogError("算法有问题?? 没有找到任何附近的坐标点");
				return 0;
			}
			CarPoint carPoint = Underline.Min(list, (CarPoint point) => (int)Vector3.Distance(carPt, point.pos.V3));
			int num2 = Math.Max(0, carPoint.index - 1);
			if (Recorder.TotalLap == 1)
			{
				float num3 = Vector3.Distance(allPoints[0].pos.V3, allPoints[num2].pos.V3);
				if (num3 < 20f && num2 > allPoints.Count / 2)
				{
					float num4 = Vector3.Distance(allPoints[0].pos.V3, carPt);
					for (int i = 1; i < allPoints.Count; i++)
					{
						float num5 = Vector3.Distance(allPoints[i].pos.V3, carPt);
						if (num5 <= num4)
						{
							num4 = num5;
							continue;
						}
						num2 = i - 1;
						break;
					}
				}
			}
			Debug.Log($"找到坐标点序号: {num2}");
			return num2;
		}

		private static List<CarPoint> FastFindRangePoints(Vector3 carPt, int step, List<CarPoint> points)
		{
			List<CarPoint> list = new List<CarPoint>();
			Vector3 vector = default(Vector3);
			vector.x = carPt.x - (float)step;
			vector.y = carPt.y - (float)step;
			vector.z = carPt.z - (float)step;
			Vector3 vector2 = vector;
			vector = default(Vector3);
			vector.x = carPt.x + (float)step;
			vector.y = carPt.y + (float)step;
			vector.z = carPt.z + (float)step;
			Vector3 vector3 = vector;
			foreach (CarPoint point in points)
			{
				Vector3Serializer pos = point.pos;
				if (pos.x > vector2.x && pos.x < vector3.x && pos.y > vector2.y && pos.y < vector3.y && pos.z > vector2.z && pos.z < vector3.z)
				{
					list.Add(point);
				}
			}
			return list;
		}

		internal void AiMove(float startTime, int startIndex, bool setPos, Action<CarView> endCallback)
		{
			if (!base.Active || pathList == null || pathList.Count < 2 || (carState.CarPlayerType != PlayerType.PALYER_AI && carState.CarPlayerType != 0))
			{
				return;
			}
			paused = false;
			float t = 0f;
			int num = ((startIndex == -1) ? tryGetNext(0, startTime, out t) : startIndex);
			if (num == -1)
			{
				LogWarning("Too much automove time.");
				paused = true;
				return;
			}
			int num2 = Math.Max(0, num - 1);
			CarPoint carPoint = pathList[num];
			if (setPos)
			{
				carView.transform.position = pathList[num2].pos.V3;
				carView.transform.rotation = pathList[num2].rot.Q;
			}
			target = new CarPoint
			{
				Flag = carPoint.Flag,
				pos = carPoint.pos
			};
			Debug.LogError("Modify by Sbin: AiMove in AiCarController , Don't subtract the height !!!");
			if (JointMove)
			{
			}
			target.rot = carPoint.rot;
			target.DeltaTime = startTime + pathList[num].DeltaTime - t;
			index = num;
			lastFlag = pathList[num2].Flag;
			lastPos = carState.rigidbody.position;
			lastRot = carState.rigidbody.rotation;
			lastTime = Time.time;
			moveStartTime = Time.time;
			carView.RotateAngleForCarModel(carPoint.rotateZ);
			vt1.Set(target.pos.x, target.pos.y, target.pos.z);
			if (target.DeltaTime != 0f)
			{
				carState.velocity = (vt1 - lastPos) / target.DeltaTime;
			}
			carState.relativeVelocity = carState.transform.InverseTransformDirection(carState.velocity);
			carState.LinearVelocity = carState.relativeVelocity.magnitude;
			carState.SpeedRatio = Mathf.Clamp01(carState.relativeVelocity.z / Mathf.Max(carState.view.carModel.MaxSpeeds[0], 1f));
			if (JointMove)
			{
				buildTargetRigidbody();
			}
			Debug.LogError("Modify by Sbin: AiMove in AiCarController , Add Start MotorTorque for every wheel collider, otherwise the car dont move at start");
			Wheel[] wheels = carView.carState.Wheels;
			foreach (Wheel wheel in wheels)
			{
				wheel.collider.motorTorque = carView.carModel.CarWeight;
			}
			LogWarning("AI Start at index ", index);
			OnEndAIMove = endCallback;
		}

		internal void StartBossAiMove(Action<CarView> endCallback = null)
		{
			autoStartTime = 0f;
			AiMoveTime(autoStartTime, setPos: true, endCallback);
		}

		internal void StartAiMove(Action<CarView> endCallback = null)
		{
			autoStartTime = 3f;
			AiMoveTime(autoStartTime, setPos: false, endCallback);
		}

		private void move(float t)
		{
			if (target != null)
			{
				vt1.Set(target.pos.x, target.pos.y, target.pos.z);
				t = Mathf.Clamp01(t);
				vt3 = Vector3.Lerp(lastPos, vt1, t);
				carState.rigidbody.MovePosition(vt3);
			}
		}

		private void rotate(float t)
		{
			if (target != null)
			{
				qt1.Set(target.rot.x, target.rot.y, target.rot.z, target.rot.w);
				qt2 = ((lastTarget != null) ? Quaternion.Slerp(lastTarget.rot.Q, target.rot.Q, t) : qt1);
				if (carView.LastRunState == RunState.Run)
				{
					continueRotTime = 0f;
					lastCollisionDirection = HitWallDirection.NONE;
				}
				if (carView.RunState == RunState.Crash && continueRotTime <= 0.5f && (lastCollisionDirection == HitWallDirection.NONE || lastCollisionDirection == carState.CollisionDirection) && (carState.CollisionDirection == HitWallDirection.LEFT || carState.CollisionDirection == HitWallDirection.RIGHT))
				{
					lastCollisionDirection = carState.CollisionDirection;
					continueRotTime += Time.deltaTime;
					float num = continueRotTime * (float)CollisionState.AiTailRotAngleSpeed;
					float num2 = ((carState.CollisionDirection == HitWallDirection.RIGHT) ? num : (0f - num));
					qt2 = Quaternion.AngleAxis(qt2.y + num2, carView.transform.up);
				}
				carState.rigidbody.MoveRotation(qt2);
			}
		}

		private void setState(int flag)
		{
			if (!RaceConfig.SelfSkidmarkOnly)
			{
				carState.view.Controller.setWheelState(carState);
			}
			else if (carState.Visible)
			{
				carState.view.Controller.setWheelState(carState);
			}
			if (((uint)flag & 4u) != 0)
			{
				carState.N2State.Level = 2;
			}
			else if (((uint)flag & 2u) != 0)
			{
				carState.N2State.Level = 1;
			}
			else if (((uint)flag & 8u) != 0)
			{
				carState.N2State.Level = 3;
			}
			else
			{
				carState.N2State.Level = 0;
			}
			if (((uint)flag & 0x10u) != 0)
			{
				carState.CurDriftState.Stage = DriftStage.ROTATING;
			}
			else
			{
				carState.CurDriftState.Stage = DriftStage.NONE;
			}
			if (((uint)flag & 0x20u) != 0)
			{
				carState.Steer = -1f;
			}
			else if (((uint)flag & 0x40u) != 0)
			{
				carState.Steer = 1f;
			}
			else
			{
				carState.Steer = 0f;
			}
			if (((uint)flag & 0x80u) != 0)
			{
				carState.Throttle = -1f;
			}
			else
			{
				carState.Throttle = 1f;
			}
		}

		private void onDriftStateChange(DriftStage lastStage)
		{
			if (lastStage == DriftStage.NONE && carState.CurDriftState.Stage != 0)
			{
				if (carState.CallBacks.OnDrift != null)
				{
					carState.CallBacks.OnDrift(CarEventState.EVENT_BEGIN);
				}
				if (carState.CallBacks.OnAIDrift != null)
				{
					carState.CallBacks.OnAIDrift(carState, CarEventState.EVENT_BEGIN);
				}
			}
			if (lastStage != 0 && carState.CurDriftState.Stage == DriftStage.NONE)
			{
				if (carState.CallBacks.OnDrift != null)
				{
					carState.CallBacks.OnDrift(CarEventState.EVENT_END);
				}
				if (carState.CallBacks.OnAIDrift != null)
				{
					carState.CallBacks.OnAIDrift(carState, CarEventState.EVENT_END);
				}
			}
		}

		private void onN2StateChange(int lastLevel)
		{
			if (lastLevel != 2 && carState.N2State.Level == 2)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
			}
			if (lastN2Level != 1 && carState.N2State.Level == 1)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
			}
			if (lastN2Level != 3 && carState.N2State.Level == 3)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
			}
			if (carState.N2State.Level == 0)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_END);
			}
		}

		private bool isBeyong(float t)
		{
			if (target == null)
			{
				return false;
			}
			if (t > target.DeltaTime / timeScale)
			{
				return true;
			}
			return false;
		}

		private void forceSet(CarPoint cp)
		{
			LogWarning("Force Set at ", cp.pos.V3, " time: ", Time.time);
			lastPos.Set(cp.pos.x, cp.pos.y, cp.pos.z);
			carState.transform.position = lastPos;
			lastRot.Set(cp.rot.x, cp.rot.y, cp.rot.z, cp.rot.w);
			carState.transform.rotation.Set(cp.rot.x, cp.rot.y, cp.rot.z, cp.rot.w);
			lastTime = Time.time;
			setState(cp.Flag);
			carState.velocity.Set(0f, 0f, 0f);
			carState.relativeVelocity.Set(0f, 0f, 0f);
			lastFlag = cp.Flag;
			carView.RotateAngleForCarModel(cp.rotateZ);
		}

		private void next()
		{
			CarPoint carPoint = ((target == null) ? pathList[index] : target);
			float t = 0f;
			int num = tryGetNext(index, Time.time - lastTime - carPoint.DeltaTime / timeScale, out t);
			if (num != -1)
			{
				lastPos.Set(carPoint.pos.x, carPoint.pos.y, carPoint.pos.z);
				lastRot.Set(carPoint.rot.x, carPoint.rot.y, carPoint.rot.z, carPoint.rot.w);
				lastFlag = carPoint.Flag;
				lastTime = Time.time - t;
				lastTarget = target ?? pathList[num];
				target = pathList[num];
				index = num;
				checkItemPoint(Time.time - moveStartTime, carPoint, target);
				checkQtePoint(Time.time - moveStartTime, carPoint, target);
				checkGasPoint(Time.time - moveStartTime, carPoint, target);
				carView.RotateAngleForCarModel(carPoint.rotateZ);
				vt1.Set(target.pos.x, target.pos.y, target.pos.z);
				if (target.DeltaTime != 0f)
				{
					carState.velocity = (vt1 - lastPos) / target.DeltaTime;
				}
				carState.relativeVelocity = carState.transform.InverseTransformDirection(carState.velocity);
				carState.LinearVelocity = carState.relativeVelocity.magnitude;
			}
			else
			{
				LogWarning("Force disable index=", index);
				Debug.LogError("Force disable index=" + index);
				forceSet(pathList[pathList.Count - 1]);
				carState.rigidbody.isKinematic = true;
				carState.view.StopAllEffects();
				carState.N2State.Level = 0;
				carState.CurDriftState.Stage = DriftStage.NONE;
				base.Active = false;
				if (OnEndAIMove != null)
				{
					OnEndAIMove(carState.view);
				}
			}
		}

		protected int tryGetNext(int startIndex, float timepass, out float t)
		{
			int num = startIndex + 1;
			t = timepass;
			int num2 = 0;
			while (num < pathList.Count && num2 <= pathList.Count)
			{
				CarPoint carPoint = pathList[num];
				if (t <= carPoint.DeltaTime / timeScale)
				{
					return num;
				}
				t -= carPoint.DeltaTime / timeScale;
				num++;
				if (closedLoop && num >= pathList.Count)
				{
					num %= pathList.Count;
					Debug.LogError("循环赛道到了路的尽头, 取模开始!");
				}
				num2++;
			}
			return -1;
		}

		private void checkItemPoint(float pastTime, CarPoint cur, CarPoint next)
		{
			if (itList.First == null)
			{
				return;
			}
			ItemToggle it = itList.First.Value;
			if (!(next.TotalPastTime >= it.time))
			{
				return;
			}
			itList.RemoveFirst();
			carState.view.CallDelay(delegate
			{
				if (carState.CallBacks.OnAiPassItemPoint != null)
				{
					carState.CallBacks.OnAiPassItemPoint(carState, it);
				}
			}, (it.time - cur.TotalPastTime) / timeScale);
		}

		private void checkQtePoint(float pastTime, CarPoint cur, CarPoint next)
		{
			if (qteList.First == null)
			{
				return;
			}
			QTEToggle qte = qteList.First.Value;
			if (!(next.TotalPastTime >= qte.time))
			{
				return;
			}
			qteList.RemoveFirst();
			carState.view.CallDelay(delegate
			{
				if (carState.CallBacks.OnAiQtePoint != null)
				{
					carState.CallBacks.OnAiQtePoint(qte.flag);
				}
			}, (qte.time - cur.TotalPastTime) / timeScale);
		}

		private void checkGasPoint(float pastTime, CarPoint cur, CarPoint next)
		{
			if (gasList.First == null)
			{
				return;
			}
			GasToggle gas = gasList.First.Value;
			if (!(next.TotalPastTime >= gas.time))
			{
				return;
			}
			gasList.RemoveFirst();
			carState.view.CallDelay(delegate
			{
				if (carState.CallBacks.OnAiGasPoint != null)
				{
					carState.CallBacks.OnAiGasPoint((float)(int)gas.percent / 100f);
				}
			}, (gas.time - cur.TotalPastTime) / timeScale);
		}

		private void backFormal(Vector3 curPos, Quaternion curRot)
		{
			if (index == pathList.Count - 1)
			{
				LogWarning("Path list too short at AI BackFormal.");
				return;
			}
			if (target == null)
			{
				LogWarning("Waypoint target is null.");
				return;
			}
			vt1 = curPos;
			CarPoint carPoint = target;
			vt2.Set(carPoint.pos.x, carPoint.pos.y, carPoint.pos.z);
			float num = float.MaxValue;
			int num2 = 20;
			int num3 = index;
			for (int i = 1; i < num2; i++)
			{
				int num4 = index + i;
				if (num4 >= pathList.Count)
				{
					break;
				}
				carPoint = pathList[num4];
				vt2.Set(carPoint.pos.x, carPoint.pos.y, carPoint.pos.z);
				float sqrMagnitude = (vt2 - vt1).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num3 = num4;
				}
			}
			float num5 = 0f;
			if (num3 != index)
			{
				for (int j = index + 1; j <= num3; j++)
				{
					carPoint = pathList[j];
					num5 += carPoint.DeltaTime;
				}
			}
			else
			{
				carPoint = target;
			}
			vt2.Set(carPoint.pos.x, carPoint.pos.y, carPoint.pos.z);
			float sqrMagnitude2 = (vt1 - vt2).sqrMagnitude;
			float num6 = Mathf.Clamp(carState.velocity.sqrMagnitude, 1f, 800f) / timeScale / timeScale;
			float num7 = Mathf.Sqrt(sqrMagnitude2 / num6);
			num5 -= num7;
			Log("%% mIndex=", num3, " lastIndex:", index, " cost:", num7, " timescale: ", timeScale, " dis: ", Mathf.Sqrt(sqrMagnitude2), " curSpeed:", num6, " ex:", num5);
			index = num3;
			target = new CarPoint();
			target.Flag = carPoint.Flag;
			target.pos = carPoint.pos;
			target.rot = carPoint.rot;
			target.DeltaTime = num7;
			lastPos = curPos;
			lastRot = curRot;
			lastTime = Time.time;
			Log("target.time:", target.DeltaTime);
		}

		protected virtual void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if (car == null || car.view != carState.view)
			{
				return;
			}
			bool isTrigger = o is PassiveTrigger;
			float value = RaceItemFactory.GetExtraTimeScale(id);
			if (value != 1f)
			{
				switch (type)
				{
					case ItemCallbackType.AFFECT:
						AddSpeedScale(value);
						if (applyScale.ContainsKey(o))
						{
							Debug.LogWarning("Duplicated Affect by item " + o);
						}
						applyScale[o] = value;
						break;
					case ItemCallbackType.BREAK:
						if (o == null)
						{
							Debug.LogError("o为空！！！");
						}
						else if (applyScale.TryGetValue(o, out value))
						{
							AddSpeedScale(1f / value);
							applyScale.Remove(o);
						}
						break;
				}
			}
			else
			{
				if (RaceItemFactory.GetDelayDuration(id, isTrigger) == 0f)
				{
					return;
				}
				switch (type)
				{
					case ItemCallbackType.BREAK:
					{
						paused = false;
						bool flag = itScaleFlag.Remove(o);
						if (JointMove)
						{
							resetJoint();
						}
						if (id == RaceItemId.BANANA && o is PassiveTrigger)
						{
							backFormal(carState.transform.position, carState.transform.rotation);
						}
						else if (flag)
						{
							lastTime += Time.time - itemDelayStartAt;
						}
						while (itScaleQueue.Count > 0)
						{
							float num = itScaleQueue.Dequeue();
							AddSpeedScale(1f / num);
						}
						if (flag)
						{
							for (int i = 0; i < 100; i++)
							{
								float num2 = 0.95f;
								AddSpeedScale(num2);
								itScaleQueue.Enqueue(num2);
							}
						}
						break;
					}
					case ItemCallbackType.AFFECT:
						itemDelayStartAt = Time.time;
						paused = true;
						itScaleFlag.Add(o);
						if (JointMove)
						{
							breakJoint();
						}
						break;
				}
			}
		}

		internal void AddSpeedScale(float mul)
		{
			if (target != null)
			{
				lastTime = ResetScaledTime(target.DeltaTime, Time.time, timeScale, timeScale * mul, lastTime);
			}
			timeScale *= mul;
		}

		internal float ResetScaledTime(float dutation, float nowT, float lastScale, float targetScale, float lastT)
		{
			return nowT - (nowT - lastT) * lastScale / targetScale;
		}

		internal void SetUpTeamGasScraper()
		{
			teamGas = new TeamGasScraper();
			teamGas.Init(carState, carState.view.carModel);
			teamGas.Active = true;
		}

		private float calTimeScale(float delay, float now, float nowScale = 1f)
		{
			float totalTime = Recorder.TotalTime;
			float num = totalTime - (now - moveStartTime);
			return num / (num + delay);
		}

		internal float GetLapStartTime(int lap)
		{
			if (pathList != null)
			{
				for (int i = 0; i < pathList.Count; i++)
				{
					if ((pathList[i].Flag & 7) >= lap)
					{
						return pathList[i].TotalPastTime;
					}
				}
			}
			return -1f;
		}

		private void onLapFinish(CarState state)
		{
			if (state == carState)
			{
				PlayerType carPlayerType = state.CarPlayerType;
				int num = 2;
			}
		}
	}
}
