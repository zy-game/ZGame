using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CarShakeController : ControllerBase
	{
		[Serializable]
		public class SpeedUpShakeParams
		{
			public float Time;

			public float UpAngle;

			public float DownAngle;

			public float TimeRatio;

			public SpeedUpShakeParams(float time, float upAngle, float downAngle, float timeRatio)
			{
				Time = time;
				UpAngle = upAngle;
				DownAngle = downAngle;
				TimeRatio = timeRatio;
			}

			public SpeedUpShakeParams(SpeedUpShakeParams args)
			{
				Time = args.Time;
				UpAngle = args.UpAngle;
				DownAngle = args.DownAngle;
				TimeRatio = args.TimeRatio;
			}
		}

		private class cUpDownShake
		{
			public bool active;

			public bool autoStop;

			public bool returningNormal;

			public float speedUpPassTime;

			public float returnNormalTime;

			public float totalTime;

			public float passTime;

			public float upTime;

			public float delyTime;

			public float downTime;

			public float startAngle;

			public float upAngle;

			public float downAngle;

			public Action downCallback;

			public float curAngle;

			public Quaternion Angle => Quaternion.Euler(curAngle, 0f, 0f);

			public void init(float time, float start, float up, float down, float timeRatio, bool autoStop, Action downCallback = null)
			{
				active = true;
				this.autoStop = autoStop;
				totalTime = time;
				startAngle = start;
				upAngle = up;
				downAngle = down;
				timeRatio = Mathf.Clamp01(timeRatio);
				delyTime = time * 0.5f;
				upTime = time * timeRatio - delyTime * 0.5f;
				downTime = time * (1f - timeRatio) - delyTime * 0.5f;
				passTime = 0f;
				this.downCallback = downCallback;
			}

			public void update()
			{
				if (!active)
				{
					return;
				}
				passTime += Time.deltaTime;
				if (passTime < upTime)
				{
					curAngle = ((upTime == 0f) ? upAngle : Mathf.Lerp(startAngle, upAngle, passTime / upTime));
				}
				else
				{
					if (passTime - upTime < delyTime)
					{
						return;
					}
					if (passTime - upTime - delyTime < downTime)
					{
						if (downCallback != null)
						{
							downCallback();
							downCallback = null;
						}
						curAngle = ((downTime == 0f) ? downAngle : Mathf.Lerp(upAngle, downAngle, (passTime - upTime - delyTime) / downTime));
					}
					else if (passTime >= totalTime)
					{
						if (downCallback != null)
						{
							downCallback();
							downCallback = null;
						}
						if (autoStop)
						{
							active = false;
						}
						curAngle = downAngle;
					}
				}
			}
		}

		private class cHitGroundShake
		{
			public bool active;

			public float totalTime;

			public float attenuation;

			public float passTime;

			public Vector3 targetPos = Vector3.zero;

			public Vector3 targetAngle = Vector3.zero;

			public cHitGroundShake nextShake;

			private Vector3 curPos = Vector3.zero;

			private Vector3 curAngle = Vector3.zero;

			public Vector3 Pos => curPos;

			public Quaternion Angle => Quaternion.Euler(curAngle);

			public void init(float time, Vector3 targetPos, Vector3 targetAngle, float attenuation)
			{
				active = true;
				totalTime = time;
				passTime = 0f;
				this.attenuation = Mathf.Clamp01(attenuation);
				this.targetPos = targetPos;
				this.targetAngle = targetAngle;
				createNext();
			}

			private void createNext()
			{
				if (totalTime * (1f - attenuation) > 0.016f)
				{
					if (nextShake == null)
					{
						nextShake = new cHitGroundShake();
					}
					nextShake.totalTime = totalTime * (1f - attenuation);
					nextShake.attenuation = attenuation;
					nextShake.targetPos = targetPos * (1f - attenuation);
					nextShake.targetAngle = targetAngle * (1f - attenuation);
				}
				else
				{
					nextShake = null;
				}
			}

			public void update()
			{
				if (active)
				{
					passTime += Time.deltaTime;
					if (passTime < totalTime)
					{
						float num = passTime / totalTime;
						num = -4f * (num - 0.5f) * (num - 0.5f) + 1f;
						curPos = targetPos * num;
						curAngle = targetAngle * num;
					}
					else if (nextShake == null)
					{
						curPos = Vector3.zero;
						curAngle = Vector3.zero;
						active = false;
					}
					else
					{
						init(nextShake.totalTime, nextShake.targetPos, nextShake.targetAngle, nextShake.attenuation);
					}
				}
			}
		}

		private static readonly List<SpeedUpShakeParams> DefaultSpeedUpShakeParams = new List<SpeedUpShakeParams>
		{
			new SpeedUpShakeParams(0.1f, 4.5f, -2f, 0.8f),
			new SpeedUpShakeParams(0.06f, 0f, -2f, 0f)
		};

		public List<SpeedUpShakeParams> speedUpShakeParams = new List<SpeedUpShakeParams>
		{
			new SpeedUpShakeParams(DefaultSpeedUpShakeParams[0]),
			new SpeedUpShakeParams(DefaultSpeedUpShakeParams[1])
		};

		private float[,] RandomArray = new float[100, 2]
		{
			{ -0.184f, 0.724f },
			{ -0.347f, -0.789f },
			{ 0.01f, -0.009f },
			{ -0.702f, 0.58f },
			{ -0.469f, -0.517f },
			{ 0.294f, 0.771f },
			{ -0.289f, -0.714f },
			{ 0.483f, -0.301f },
			{ -0.823f, -0.738f },
			{ 0.543f, 0.863f },
			{ 0.696f, -0.929f },
			{ -0.674f, 0.552f },
			{ -0.233f, -0.879f },
			{ 0.048f, 0.119f },
			{ -0.437f, 0.408f },
			{ -0.838f, 0.781f },
			{ -0.187f, 0.113f },
			{ 0.695f, -0.373f },
			{ 0.77f, 0.253f },
			{ -0.828f, -0.088f },
			{ -0.783f, -0.921f },
			{ -0.933f, 0.561f },
			{ -0.68f, 0.889f },
			{ -0.054f, 0.593f },
			{ -0.789f, -0.899f },
			{ -0.157f, -0.208f },
			{ 0.853f, 0.873f },
			{ -0.573f, 0.649f },
			{ 0.792f, -0.534f },
			{ 0.773f, -0.611f },
			{ -0.795f, 0.876f },
			{ -0.437f, -0.108f },
			{ 0.841f, 0.421f },
			{ 0.247f, 0.153f },
			{ 0.355f, -0.148f },
			{ -0.021f, -0.26f },
			{ 0.094f, -0.552f },
			{ -0.227f, -0.146f },
			{ -0.515f, -0.278f },
			{ 0.227f, 0.141f },
			{ -0.507f, 0.827f },
			{ -0.23f, -0.301f },
			{ -0.815f, -0.214f },
			{ -0.166f, -0.211f },
			{ -0.361f, 0.33f },
			{ 0.799f, 0.802f },
			{ -0.713f, 0.044f },
			{ -0.571f, 0.508f },
			{ 0.924f, 0.073f },
			{ -0.476f, -0.831f },
			{ 0.45f, -0.388f },
			{ -0.046f, -0.551f },
			{ -0.36f, 0.661f },
			{ -0.286f, -0.906f },
			{ -0.323f, -0.71f },
			{ -0.221f, 0.221f },
			{ 0.79f, 0.355f },
			{ 0.619f, -0.638f },
			{ 0.51f, -0.154f },
			{ 0.427f, -0.901f },
			{ 0.578f, 0.807f },
			{ 0.429f, 0.391f },
			{ 0.28f, -0.942f },
			{ 0.349f, 0.911f },
			{ -0.022f, -0.54f },
			{ -0.878f, -0.546f },
			{ -0.215f, 0.821f },
			{ 0.179f, -0.807f },
			{ -0.127f, 0.861f },
			{ 0.999f, -0.369f },
			{ -0.242f, 0.585f },
			{ -0.722f, 0.37f },
			{ -0.461f, -0.689f },
			{ 0.113f, -0.964f },
			{ -0.212f, 0.692f },
			{ -0.235f, 0.075f },
			{ 0.506f, -0.566f },
			{ 0.256f, 0.856f },
			{ -0.395f, -0.776f },
			{ 0.922f, 0.039f },
			{ -0.523f, -0.415f },
			{ -0.321f, 0.301f },
			{ -0.446f, 0.037f },
			{ -0.883f, 0.499f },
			{ -0.904f, 0.829f },
			{ -0.743f, -0.709f },
			{ 0.211f, 0.823f },
			{ -0.829f, 0.115f },
			{ 0.303f, -0.243f },
			{ 0.839f, 0.109f },
			{ 0.633f, 0.998f },
			{ -0.355f, -0.275f },
			{ 0.245f, 0.986f },
			{ -0.454f, -0.846f },
			{ 0.402f, -0.307f },
			{ -0.298f, -0.28f },
			{ -0.502f, 0.546f },
			{ 0.314f, 0.845f },
			{ -0.035f, -0.432f },
			{ 0.062f, -0.518f }
		};

		public const string ShakeRootName = "ShakeRoot";

		public const float RandomShakeAngle = 0.2f;

		private cUpDownShake upDownShake = new cUpDownShake();

		private cHitGroundShake hitGroundShake = new cHitGroundShake();

		private CarState controlCar;

		private Transform shakeRoot;

		public Transform tranModel;

		public Transform tranN2Gas;

		private bool isfly_preFrame;

		private float dragDownSpeed;

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			if (controlCar == null)
			{
				return;
			}
			if (active)
			{
				setupChildren(controlCar.view.CarBody);
				if (controlCar.view.ShakeEffects != null)
				{
					for (int i = 0; i < controlCar.view.ShakeEffects.Count; i++)
					{
						Transform target = controlCar.view.ShakeEffects[i];
						setupChildren(target);
					}
				}
				CarView view = controlCar.view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(UpdateShake));
				if (controlCar.view.carModel.carModelType == CarModelType.Motorbike)
				{
					CarView view2 = controlCar.view;
					view2.OnFixedupdate = (Action)Delegate.Combine(view2.OnFixedupdate, new Action(RotateCar));
				}
			}
			else
			{
				uninstallCarBody();
				CarView view3 = controlCar.view;
				view3.OnUpdate = (Action)Delegate.Remove(view3.OnUpdate, new Action(UpdateShake));
				if (controlCar.view.carModel.carModelType == CarModelType.Motorbike)
				{
					CarView view4 = controlCar.view;
					view4.OnFixedupdate = (Action)Delegate.Remove(view4.OnFixedupdate, new Action(RotateCar));
				}
			}
		}

		public void Init(CarState carState)
		{
			if (carState == null)
			{
				LogError("in function CarShakeController.Init, error: param 1, null is not allow.");
			}
			else
			{
				controlCar = carState;
			}
		}

		public void StartSpeedUpShake(float time, float upAngle, float downAngle, float timeRatio, Action downCallback = null)
		{
			upDownShake.init(time, 0f, upAngle, downAngle, timeRatio, autoStop: false, downCallback);
		}

		public void StartSpeedUpShake(int level, Action downCallback)
		{
			SpeedUpShakeParams speedUpShakeParams = this.speedUpShakeParams[0];
			if (level != 0 && 1 != level)
			{
				speedUpShakeParams = this.speedUpShakeParams[1];
			}
			StartSpeedUpShake(speedUpShakeParams.Time, speedUpShakeParams.UpAngle, speedUpShakeParams.DownAngle, speedUpShakeParams.TimeRatio, downCallback);
		}

		public void EndSpeedUpShake(float time)
		{
			upDownShake.init(time, upDownShake.curAngle, 0f, 0f, 1f, autoStop: true);
		}

		public void HitGroundShake(float force)
		{
			if (!upDownShake.active)
			{
				float num = Mathf.Clamp(force, 0f, 100f) * 0.01f;
				hitGroundShake.init(0.3f * num, new Vector3(0f, 0.04f * num, 0f), new Vector3(1.7f * num, 0f, 0f), 0.5f);
			}
		}

		private void setupChildren(Transform target)
		{
			if (!(target == null) && !(shakeRoot == target.parent))
			{
				if (shakeRoot == null)
				{
					shakeRoot = new GameObject("ShakeRoot").transform;
					shakeRoot.parent = controlCar.transform;
					shakeRoot.localPosition = Vector3.zero;
					shakeRoot.localRotation = Quaternion.identity;
					shakeRoot.localScale = Vector3.one;
				}
				target.parent = shakeRoot;
				tranModel = shakeRoot.Find("Models");
			}
		}

		private void uninstallCarBody()
		{
			if (!(shakeRoot == null))
			{
				shakeRoot.localPosition = Vector3.zero;
				shakeRoot.localRotation = Quaternion.identity;
				shakeRoot.localScale = Vector3.one;
				for (int num = shakeRoot.childCount - 1; num >= 0; num--)
				{
					Transform child = shakeRoot.GetChild(num);
					child.parent = shakeRoot.parent;
				}
				UnityEngine.Object.DestroyImmediate(shakeRoot.gameObject);
				shakeRoot = null;
			}
		}

		private void UpdateShake()
		{
			if (controlCar != null && !(shakeRoot == null))
			{
				RandomShake();
				UpDownShake();
				if (isHitGround())
				{
					HitGroundShake(100f);
				}
				isfly_preFrame = controlCar.GroundHit == 0;
				if (isfly_preFrame)
				{
					dragDownSpeed = controlCar.velocity.y;
				}
				HitGroundShake();
			}
		}

		private void RandomShake()
		{
			if (!upDownShake.active && !hitGroundShake.active)
			{
				float num = 1f;
				if (num > 0f)
				{
					int num2 = (int)(Time.time * 1000f) % RandomArray.GetLength(0);
					float x = RandomArray[num2, 0] * 0.2f * num;
					float z = RandomArray[num2, 1] * 0.2f * num;
					shakeRoot.localRotation = Quaternion.Euler(x, 0f, z);
				}
				else
				{
					shakeRoot.localRotation = Quaternion.identity;
				}
			}
		}

		private void UpDownShake()
		{
			if (upDownShake.active)
			{
				upDownShake.update();
				shakeRoot.localRotation = upDownShake.Angle;
			}
		}

		private void HitGroundShake()
		{
			if (!upDownShake.active && hitGroundShake.active)
			{
				hitGroundShake.update();
				shakeRoot.localPosition = hitGroundShake.Pos;
				shakeRoot.localRotation = hitGroundShake.Angle;
			}
		}

		private bool isHitGround()
		{
			if (isfly_preFrame && controlCar.GroundHit != 0 && Mathf.Abs(dragDownSpeed) > 8f)
			{
				return true;
			}
			return false;
		}

		private void RotateCar()
		{
			if (!(controlCar.view.carModel.TurnRotateAngle == 0f) || !(controlCar.view.carModel.DriftRotateAngle == 0f))
			{
				float num = Mathf.Clamp01((Time.time - controlCar.view.carState.SteerChangeTime) / (float)controlCar.view.carModel.FullRotateTime);
				if (controlCar.view.carState.CurDriftState.Stage != 0)
				{
					float angle = num * (float)controlCar.view.carModel.DriftRotateAngle;
					rotateToAngle(angle, tranModel);
				}
				else if (controlCar.view.carState.CurDriftState.Stage == DriftStage.NONE && controlCar.view.carState.Steer == 0f)
				{
					rotateToAngle(0f, tranModel);
				}
				else if (controlCar.view.carState.CurDriftState.Stage == DriftStage.NONE && controlCar.view.carState.Steer != 0f && controlCar.view.carState.Throttle != 0f)
				{
					float angle2 = num * (float)controlCar.view.carModel.TurnRotateAngle;
					rotateToAngle(angle2, tranModel);
				}
			}
		}

		private void rotateToAngle(float angle, Transform tranModel)
		{
			if (tranModel == null)
			{
				tranModel = this.tranModel;
			}
			angle *= 0f - controlCar.view.carState.Steer;
			float num = tranModel.localRotation.eulerAngles.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			if (Mathf.Abs(num - angle) >= 1f)
			{
				angle = Mathf.Lerp(num, angle, Time.deltaTime * 2f);
			}
			Quaternion quater = Quaternion.Euler(0f, 0f, angle);
			controlCar.view.RotateAngleForCarModel(quater);
		}
	}
}
