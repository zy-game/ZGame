using System;
using System.Collections;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CollisionController : ControllerBase
	{
		public delegate bool TriggerByAICheckerDelegate(CarState selfCarState, CarState otherCarState);

		public string WallTag = "Wall";

		private CarView carView;

		private CarState carState;

		private EffectToggleBase sparkEffect;

		private bool sparkInited;

		private bool invalidRoad;

		private int sparkFlag;

		private float lastSparkTime;

		private int layerCar;

		private int layerWheel;

		private int layerWall;

		private IEnumerator coCollideStateWait;

		private RigidbodyConstraints lastConstraints;

		private float lastTriggerTime;

		private float triggerBeginTime;

		private bool isTriggerAi;

		private CarState aiCarState;

		private int rotAngle;

		private bool nextFirst = true;

		private float rotSpeed;

		private float lastPushTime;

		public TriggerByAICheckerDelegate CheckTriggerByAI;

		public static Vector3 collDir = Vector3.zero;

		public bool InvalidRoad => invalidRoad;

		public void Init(CarView view, CarState state)
		{
			carView = view;
			carState = state;
			layerCar = LayerMask.NameToLayer("CarCollider");
			layerWheel = LayerMask.NameToLayer("Wheel");
			layerWall = 9;
		}

		public void FixedUpdate()
		{
			if (carView.CarPlayerType != 0)
			{
				return;
			}
			if (isTriggerAi && carState != null)
			{
				bool firstEnter = false;
				if (nextFirst)
				{
					firstEnter = true;
					nextFirst = false;
				}
				if (Time.realtimeSinceStartup - triggerBeginTime > 1f || (CheckTriggerByAI != null && !CheckTriggerByAI(carState, aiCarState)))
				{
					carView.crasherController.isTriggerAi = false;
					carView.RunState = RunState.Run;
					carView.crasherController.aiCarState = null;
					return;
				}
				DoTriggerByAi(aiCarState, firstEnter);
			}
			if (!isTriggerAi)
			{
				nextFirst = true;
			}
			if (carView.CarPlayerType == PlayerType.PLAYER_SELF && isTriggerAi)
			{
				Debug.Log(" --- CollisionController in CarLogic.Dll ---  Modify by LiuShibin");
				carView.GetComponent<Rigidbody>().MovePosition(FallingToTheGround(carView.transform.position));
			}
		}

		private Vector3 FallingToTheGround(Vector3 pos)
		{
			CharacterController component = carState.view.GetComponent<CharacterController>();
			if ((bool)component && Physics.Raycast(pos + new Vector3(0f, 2f, 0f), Vector3.down, out var hitInfo, 100f, 256))
			{
				Vector3 point = hitInfo.point;
				point.y += 0f - component.center.y + component.radius + 0.05f;
				if (point.y + 0.3f >= pos.y)
				{
					pos = point;
				}
			}
			return pos;
		}

		public override void OnActiveChange(bool active)
		{
			if (active)
			{
				CarView carView = this.carView;
				carView.OnCollisionBegin = (Action<Collision>)Delegate.Combine(carView.OnCollisionBegin, new Action<Collision>(OnCollisionEnter));
				CarView carView2 = this.carView;
				carView2.OnCollisionEnd = (Action<Collision>)Delegate.Combine(carView2.OnCollisionEnd, new Action<Collision>(OnCollisionExit));
				if ((bool)SyncMoveController.OpenNewCollision)
				{
					CarView carView3 = this.carView;
					carView3.OnTriggerBegin = (Action<Collider>)Delegate.Combine(carView3.OnTriggerBegin, new Action<Collider>(OnTriggerBegin));
					CarView carView4 = this.carView;
					carView4.OnTriggerEnd = (Action<Collider>)Delegate.Combine(carView4.OnTriggerEnd, new Action<Collider>(OnTriggerEnd));
				}
			}
			else
			{
				CarView carView5 = this.carView;
				carView5.OnCollisionBegin = (Action<Collision>)Delegate.Remove(carView5.OnCollisionBegin, new Action<Collision>(OnCollisionEnter));
				CarView carView6 = this.carView;
				carView6.OnCollisionEnd = (Action<Collision>)Delegate.Remove(carView6.OnCollisionEnd, new Action<Collision>(OnCollisionExit));
				CarView carView7 = this.carView;
				carView7.OnTriggerBegin = (Action<Collider>)Delegate.Remove(carView7.OnTriggerBegin, new Action<Collider>(OnTriggerBegin));
				CarView carView8 = this.carView;
				carView8.OnTriggerEnd = (Action<Collider>)Delegate.Remove(carView8.OnTriggerEnd, new Action<Collider>(OnTriggerEnd));
			}
		}

		private CarView FindCarView(GameObject go)
		{
			Transform parent = go.transform.parent;
			if (parent != null)
			{
				return FindCarView(parent.gameObject);
			}
			return go.GetComponent<CarView>();
		}

		private void OnTriggerBegin(Collider collider)
		{
			if (!base.Active || (carState.CarPlayerType != PlayerType.PALYER_AI && carState.CarPlayerType != PlayerType.PLAYER_OTHER && carState.CarPlayerType != PlayerType.PLAYER_AUTO_AI))
			{
				return;
			}
			GameObject gameObject = collider.gameObject;
			int layer = gameObject.layer;
			if (layer == layerCar)
			{
				CarView carView = FindCarView(gameObject);
				if ((bool)carView && carView.CarPlayerType == PlayerType.PLAYER_SELF && carView.carState.ApplyingSpecialType != SpecialType.Translate)
				{
					carView.crasherController.triggerBeginTime = Time.realtimeSinceStartup;
					carView.crasherController.isTriggerAi = true;
					carView.RunState = RunState.Crash;
					carView.crasherController.aiCarState = carState;
				}
			}
		}

		private void OnTriggerEnd(Collider collider)
		{
			if (!base.Active || (carState.CarPlayerType != PlayerType.PALYER_AI && carState.CarPlayerType != PlayerType.PLAYER_OTHER && carState.CarPlayerType != PlayerType.PLAYER_AUTO_AI))
			{
				return;
			}
			GameObject gameObject = collider.gameObject;
			int layer = gameObject.layer;
			if (layer == layerCar)
			{
				CarView carView = FindCarView(gameObject);
				if ((bool)carView && carView.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					carView.crasherController.isTriggerAi = false;
					carView.RunState = RunState.Run;
					carView.crasherController.aiCarState = null;
				}
			}
		}

		public void DoTriggerByAi(CarState otherCarState, bool firstEnter)
		{
			Vector3 normalized = (carView.transform.position - otherCarState.LastPosition).normalized;
			HitWallDirection hitWallDirection = otherCarState.CollisionState.DoCollisionDirection(normalized);
			if (Vector3.Dot(carView.transform.forward, normalized) > 0f)
			{
				switch (hitWallDirection)
				{
					case HitWallDirection.FRONT:
						rotSpeed = 2f;
						rotAngle = 0;
						break;
					case HitWallDirection.LEFT:
						rotSpeed = CollisionState.AiSideRotAngleSpeed;
						rotAngle = -1;
						break;
					case HitWallDirection.RIGHT:
						rotSpeed = CollisionState.AiSideRotAngleSpeed;
						rotAngle = 1;
						break;
				}
			}
			else
			{
				rotSpeed = CollisionState.AiTailRotAngleSpeed;
				if (Vector3.Dot(carState.transform.right, normalized) > 0f)
				{
					rotAngle = 1;
				}
				else
				{
					rotAngle = -1;
				}
			}
			float num = Mathf.Abs(Quaternion.FromToRotation(carState.transform.forward, -normalized).eulerAngles.y);
			if (num > 180f)
			{
				num = Mathf.Abs(num - 360f);
			}
			float num2 = 45f - num;
			if (num2 < 0f)
			{
				num2 = 1f;
			}
			float num3 = Quaternion.FromToRotation(otherCarState.transform.forward, carState.transform.forward).eulerAngles.y;
			if (Mathf.Abs(num3) > 180f)
			{
				num3 -= 360f;
			}
			if ((rotAngle > 0 && num3 < num2) || (rotAngle < 0 && num3 > 0f - num2))
			{
				DoRotAngle(rotAngle);
			}
			DoPlaySpark(carView.GetComponent<Rigidbody>().velocity - otherCarState.rigidbody.velocity, bounce: true);
			if (Time.realtimeSinceStartup - lastTriggerTime < 0.1f)
			{
				return;
			}
			lastTriggerTime = Time.realtimeSinceStartup;
			bool flag = otherCarState.velocity.sqrMagnitude > carState.rigidbody.velocity.sqrMagnitude;
			if (Vector3.Dot(carView.transform.forward, normalized) > 0f)
			{
				switch (hitWallDirection)
				{
					case HitWallDirection.FRONT:
						if (flag && Time.time - lastPushTime > 1f)
						{
							lastPushTime = Time.time;
							Vector3 velocity = carState.rigidbody.velocity;
							Vector3 vector = Vector3.Project(velocity * CollisionState.OtherPileupVelocityFactor, normalized);
							float value = CollisionState.OtherPileupVelocityCurve[CollisionState.OtherPileupVelocityCurve.length - 1].value;
							float num4 = CollisionState.OtherPileupVelocityCurve.Evaluate(vector.magnitude * 3.6f / value);
							vector = vector.normalized * (num4 / 3.6f);
							Vector3 velocity2 = velocity + vector;
							Debug.Log(" --- CollisionController in CarLogic.Dll ---  Modify by LiuShibin");
							carView.GetComponent<Rigidbody>().velocity = velocity2;
							carView.carState.velocity = carView.GetComponent<Rigidbody>().velocity;
						}
						break;
					case HitWallDirection.LEFT:
						DoAiBounce(otherCarState);
						break;
					case HitWallDirection.RIGHT:
						DoAiBounce(otherCarState);
						break;
				}
			}
			else if (Time.time - lastPushTime > 1f)
			{
				lastPushTime = Time.time;
				Vector3 velocity3 = carView.carState.velocity;
				Debug.Log(" --- CollisionController in CarLogic.Dll ---  Modify by LiuShibin");
				carView.GetComponent<Rigidbody>().velocity *= 1f - (float)CollisionState.AiFaceBounceVelocityFactor;
				carView.carState.velocity = carView.GetComponent<Rigidbody>().velocity;
				DLogError("原速度: {0}, 新速度: {1}", velocity3.magnitude, carView.carState.velocity.magnitude);
			}
			if (carState.CallBacks.OnHitCar != null)
			{
				carState.CallBacks.OnHitCar(carState, otherCarState);
			}
		}

		private void DoAiBounce(CarState aiCarState)
		{
			Vector3 velocity = carState.rigidbody.velocity;
			Vector3 onNormal = carState.transform.position - aiCarState.transform.position;
			Vector3 rhs = Vector3.Project(velocity * CollisionState.OtherPileupVelocityFactor, onNormal);
			if (Time.time - lastPushTime > 1f)
			{
				lastPushTime = Time.time;
				Debug.Log(" --- CollisionController in CarLogic.Dll ---  Modify by LiuShibin");
				carView.GetComponent<Rigidbody>().velocity *= 1f - (float)CollisionState.AiFaceBounceVelocityFactor;
				carState.velocity = carState.rigidbody.velocity;
			}
			if (Vector3.Dot(velocity, rhs) <= 0f)
			{
				float value = CollisionState.OtherPileupVelocityCurve[CollisionState.OtherPileupVelocityCurve.length - 1].value;
				float num = CollisionState.OtherPileupVelocityCurve.Evaluate(rhs.magnitude * 3.6f / value);
				rhs = rhs.normalized * (num / 3.6f);
				carState.rigidbody.velocity += rhs;
				carState.velocity = carState.rigidbody.velocity;
				DLogError("原速度: {0}, 新速度: {1}", velocity.magnitude, carView.carState.velocity.magnitude);
			}
		}

		private void DoRotAngle(int sign)
		{
			if (sign != 0)
			{
				float num = Time.deltaTime * rotSpeed;
				float y = (float)sign * num;
				Debug.Log(" --- CollisionController in CarLogic.Dll ---  Modify by LiuShibin");
				carView.GetComponent<Rigidbody>().rotation *= Quaternion.Euler(0f, y, 0f);
				carView.GetComponent<Rigidbody>().velocity = Vector3.Project(carView.GetComponent<Rigidbody>().velocity, carView.transform.forward);
				carState.velocity = carView.GetComponent<Rigidbody>().velocity;
			}
		}

		private void OnCollisionStay(Collision collision)
		{
			Debug.Log("OnCollisionStay:" + collision.gameObject.name);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!base.Active)
			{
				return;
			}
			GameObject gameObject = collision.collider.gameObject;
			int layer = gameObject.layer;
			if (layer != 9 && layer != layerCar)
			{
				return;
			}
			switch (carView.CarPlayerType)
			{
				case PlayerType.PALYER_AI:
					if (layer == 9)
					{
						return;
					}
					break;
				case PlayerType.PLAYER_OTHER:
					if (layer == 9)
					{
						return;
					}
					break;
				case PlayerType.PLAYER_SELF:
					if (carView.carState.Throttle < 0f)
					{
						return;
					}
					break;
			}
			Vector3 normal = collision.contacts[0].normal;
			Vector3 rhs = carState.LastRotation * Vector3.forward;
			Vector3 vector = Vector3.Project(normal, carState.transform.up);
			Vector3 vector2 = normal - vector;
			carState.HitNormal = vector2.normalized;
			carState.HitDot = Vector3.Dot(normal, rhs);
			carState.HitVelocity = collision.relativeVelocity;
			carState.HitPoint = collision.contacts[0].point;
			Vector3 normalized = (collision.contacts[0].point - carState.LastPosition).normalized;
			float num = Vector3.Dot(normalized, rhs);
			carView.carState.CollisionState.DoCollisionDirection(normalized);
			bool flag = false;
			if (layer != layerCar && layer != layerWheel)
			{
				if (layer == layerWall && num > 0f)
				{
					flag = OnCollisionByWall();
				}
				if (coCollideStateWait != null)
				{
					carView.StopCoroutine(coCollideStateWait);
					coCollideStateWait = null;
				}
				carView.StartCoroutine(coCollideStateWait = CoCollideStateWait(0.5f, delegate
				{
					carView.RunState = RunState.Run;
					ClearHitInfos();
					coCollideStateWait = null;
				}));
				if (layer == layerWall && carState.CallBacks.OnHitWall != null)
				{
					carState.CallBacks.OnHitWall(carState);
				}
				DoPlaySpark(collision.relativeVelocity, bounce: true);
			}
		}

		private void DoPlaySpark(Vector3 deltaVoelocity, bool bounce)
		{
			if (deltaVoelocity.sqrMagnitude > RaceConfig.CrashSparkSpeed * RaceConfig.CrashSparkSpeed && bounce && Time.time - lastSparkTime > RaceConfig.SparkInterval)
			{
				PlaySparkEffect();
				PlayCollideAudioEffect();
			}
		}

		private bool OnCollisionByWall()
		{
			bool result = true;
			if (carView.RunState != 0)
			{
				return false;
			}
			if (carView.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				DLogError("撞墙");
				CollisionState collisionState = carView.carState.CollisionState;
				Vector3 velocity = carState.rigidbody.velocity;
				float num = Vector3.Angle(carState.transform.forward, -carState.HitNormal);
				int num2 = ((carState.CollisionDirection == HitWallDirection.LEFT) ? 1 : (-1));
				DLogError(num);
				if ((int)carView.carState.Steer != 0)
				{
					num2 = Math.Sign(carView.carState.Steer);
				}
				Vector3 normalized = Vector3.Cross(num2 * carState.HitNormal, carState.transform.up).normalized;
				float num3 = Vector3.Angle(carState.transform.forward, normalized);
				if (num < (float)CollisionState.FaceBounceAngle)
				{
					DLogError("正怼");
					result = collisionState.DoFaceBounce(velocity, CollisionState.FaceBounceVelocityFactor, slide: true);
					if (num < 45f)
					{
						DLogError("正怼扭正:{0}", num3);
						collisionState.RotAngle = (float)num2 * (Mathf.Abs(num3) / 2f + (float)CollisionState.OutBounceAngleOffset);
					}
				}
				else if (num > 90f)
				{
					DLogError("甩尾");
					collisionState.DoSideBounce(velocity, CollisionState.TailBounceVelocityFactor);
					result = true;
				}
				else
				{
					DLogError("侧碰");
					result = collisionState.DoSideBounce(velocity, CollisionState.SideBounceVelocityFactor);
					DLogError("侧碰扭正");
					collisionState.RotAngle = (float)num2 * (float)CollisionState.OutBounceSideAngleOffset;
				}
			}
			return result;
		}

		private void ClearHitInfos()
		{
			carView.carState.HitDot = 0f;
			carView.carState.HitNormal = Vector3.zero;
			carView.carState.HitPoint = Vector3.zero;
			carView.carState.HitVelocity = Vector3.zero;
		}

		private IEnumerator CoCollideStateWait(float delayTime, Action callback)
		{
			carView.RunState = RunState.Crash;
			yield return new WaitForSeconds(delayTime);
			callback();
		}

		private void PlaySparkEffect()
		{
			lastSparkTime = Time.time;
			carState.view.PlayOnCrash();
			if (sparkEffect == null && !sparkInited)
			{
				sparkInited = true;
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.CrashSpark, delegate(UnityEngine.Object o)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					if (gameObject != null && carState.transform != null)
					{
						sparkEffect = gameObject.AddComponent<EffectToggleBase>();
						gameObject.transform.parent = carState.view.transform;
						gameObject.transform.localPosition = new Vector3(0f, 0.1f, -0.5f);
						sparkEffect.Init();
						sparkEffect.Active = true;
						int flag2 = ++sparkFlag;
						carState.view.CallDelay(delegate
						{
							if (flag2 == sparkFlag)
							{
								sparkEffect.Active = false;
							}
						}, 0.3f);
					}
				});
			}
			else
			{
				if (!(sparkEffect != null))
				{
					return;
				}
				sparkEffect.Active = true;
				int flag = ++sparkFlag;
				carState.view.CallDelay(delegate
				{
					if (flag == sparkFlag)
					{
						sparkEffect.Active = false;
					}
				}, 0.3f);
			}
		}

		private void PlayCollideAudioEffect()
		{
			if (null != carState.view.ExEffectSource && RaceAudioManager.ActiveInstance != null)
			{
				carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_crash);
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (base.Active)
			{
				int layer = collision.collider.gameObject.layer;
				if (layer == 9 || layer == layerCar)
				{
					bool flag = false;
					carState.CollisionDirection = HitWallDirection.NONE;
				}
			}
		}

		private void DLogError(object message, params object[] args)
		{
		}

		public void OnReset()
		{
			ClearHitInfos();
			carView.RunState = RunState.Run;
			carView.crasherController.isTriggerAi = false;
			carView.crasherController.aiCarState = null;
			carState.CollisionState.OnReset();
		}
	}
}
