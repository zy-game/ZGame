using System;
using CarLogic;
using UnityEngine;

[AddComponentMenu("CarLogic/Camera-Controller")]
public class CameraController : MonoBehaviour
{
	public enum ShakeType
	{
		X,
		Y,
		XnY,
		Random,
		YBack
	}

	internal enum LeanType
	{
		None,
		Left,
		Right,
		Reset
	}

	public bool AutoSetSkybox = true;

	public float distance = 10f;

	public float N2Distance = 3f;

	public float height = 5f;

	public float PosDamping = 1f;

	public float rotationDamping = 3f;

	public float NormalFieldOfView = 60f;

	public float MinFieldOfView = 40f;

	public float MaxFieldOfView = 90f;

	public float SmallGasMaxFieldOfView = 90f;

	public float CloseHeight = 0.5f;

	public float SlopeHeight = 0.5f;

	public float FOVUpFactor = 0.1f;

	public float FOVDownFactor = 0.7f;

	public float FOVSmallGasUpScale = 1f;

	public AnimationCurve N2oUpFactorCurve = new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(1f, 0.7f));

	public AnimationCurve SmallGasUpFactorCurve = new AnimationCurve(new Keyframe(0f, 0.1f), new Keyframe(1f, 0.1f));

	public AnimationCurve AirDownFactorCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0f, 0f));

	private int preN2StateLevel = -1;

	private float N2oUpTime;

	private CarState state;

	private float time;

	private Camera viewCamera;

	private float curRotDamp;

	private bool damping;

	private float dampingTmp;

	private float velDampTime = 0.5f;

	private Skybox skybox;

	private Vector3 lastNormal = Vector3.up;

	private float lastHightOffset;

	private float airDownTime;

	public static CameraController Current;

	public bool Freeview;

	[NonSerialized]
	public Quaternion ExRotation = Quaternion.identity;

	public bool look = true;

	public bool dampRotation = true;

	[NonSerialized]
	public bool UseVelocityDir;

	private Quaternion currentRotation;

	private Vector3 currentUpward = Vector3.up;

	private Vector3 vf = Vector3.forward;

	private static Quaternion qUp = Quaternion.Euler(0f, 1f, 0f);

	private static Quaternion[] xShakeOffset = new Quaternion[2]
	{
		Quaternion.Euler(0f, 1f, 0f),
		Quaternion.Euler(0f, -1f, 0f)
	};

	private static Quaternion[] yShakeOffset = new Quaternion[2]
	{
		Quaternion.Euler(1f, 0f, 0f),
		Quaternion.Euler(-1f, 0f, 0f)
	};

	private static Quaternion[] yBackOffset = new Quaternion[1] { Quaternion.Euler(1f, 0f, 0f) };

	private static Quaternion[] xyShakeOffset = new Quaternion[2]
	{
		Quaternion.Euler(0f, 0f, 3f),
		Quaternion.Euler(0f, 0f, -3f)
	};

	private static Quaternion[] rShakeOffset = new Quaternion[4]
	{
		Quaternion.Euler(1f, 1f, 0f),
		Quaternion.Euler(-1f, -1f, 0f),
		Quaternion.Euler(1f, -1f, 0f),
		Quaternion.Euler(-1f, -1f, 0f)
	};

	private bool shaking;

	private static Quaternion identity = Quaternion.identity;

	private float timeFactor = 0.8f;

	private LeanType leanState;

	private Quaternion startAt;

	private Quaternion target;

	private float startTime;

	private AnimationCurve cur;

	public AnimationCurve LeanCurve;

	public AnimationCurve BackCurve;

	public Camera ViewCamera => viewCamera;

	public CarState ViewTarget => state;

	public void Awake()
	{
		Current = this;
		viewCamera = GetComponentInChildren<Camera>();
	}

	public void Start()
	{
		if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			FreeCamera freeCamera = base.gameObject.GetComponent<FreeCamera>();
			if (freeCamera == null)
			{
				freeCamera = base.gameObject.AddComponent<FreeCamera>();
			}
			freeCamera.SetCamera(this);
		}
	}

	public void OnDestroy()
	{
		if (this == Current)
		{
			Current = null;
		}
		state = null;
		if (viewCamera != null && viewCamera.targetTexture != null)
		{
			viewCamera.targetTexture.Release();
			viewCamera.targetTexture = null;
		}
	}

	public void Init(AbstractView view, CarState state)
	{
		this.state = state;
		ResetCurRotDamp();
		if (RacePathManager.ActiveInstance != null)
		{
			SetSkybox(RacePathManager.ActiveInstance.SkyboxMaterial);
		}
		currentRotation = state.transform.localRotation;
	}

	public void SetSkybox(Material mat)
	{
		if (skybox == null)
		{
			skybox = ViewCamera.GetComponent<Skybox>();
		}
		if (skybox != null)
		{
			skybox.material = mat;
		}
	}

	public void SetCurRotDamp(float f, bool needDamp = false)
	{
		curRotDamp = f;
		damping = needDamp;
	}

	public void ResetCurRotDamp(float dTime = 0.5f)
	{
		damping = true;
		velDampTime = dTime;
	}

	private void dampRotVelocity()
	{
		if (damping)
		{
			curRotDamp = Mathf.SmoothDamp(curRotDamp, rotationDamping, ref dampingTmp, velDampTime);
			if (Mathf.Abs(curRotDamp - rotationDamping) < 0.1f)
			{
				damping = false;
			}
		}
	}

	public void FixedUpdate()
	{
		UpdateCamera();
		ApplyLeaning();
	}

	private void UpdateCameraFov()
	{
		float normalFieldOfView = NormalFieldOfView;
		float fOVDownFactor = FOVDownFactor;
		float num = FOVUpFactor;
		if (state.N2State.Level > 0)
		{
			if (preN2StateLevel != state.N2State.Level)
			{
				N2oUpTime = 0f;
			}
			else
			{
				N2oUpTime += Time.deltaTime;
			}
			if (state.N2State.Level == 1)
			{
				normalFieldOfView = MaxFieldOfView;
				num = N2oUpFactorCurve.Evaluate(N2oUpTime);
			}
			else
			{
				normalFieldOfView = SmallGasMaxFieldOfView;
				num = SmallGasUpFactorCurve.Evaluate(N2oUpTime) * FOVSmallGasUpScale;
			}
		}
		else if (state.AirGround.DoingGas)
		{
			normalFieldOfView = SmallGasMaxFieldOfView;
			num = SmallGasUpFactorCurve.Evaluate(N2oUpTime) * FOVSmallGasUpScale;
		}
		else
		{
			N2oUpTime = 0f;
			normalFieldOfView = MinFieldOfView + (NormalFieldOfView - MinFieldOfView) * Mathf.Abs(state.SpeedRatio);
		}
		preN2StateLevel = state.N2State.Level;
		normalFieldOfView = Mathf.Clamp(normalFieldOfView, MinFieldOfView, MaxFieldOfView);
		viewCamera.fieldOfView = Mathf.SmoothDamp(viewCamera.fieldOfView, normalFieldOfView, ref time, (viewCamera.fieldOfView > normalFieldOfView) ? fOVDownFactor : num);
	}

	private float UpdateCameraAirDownHeigh()
	{
		float result = 0f;
		if (state.GroundHit == 0)
		{
			result = AirDownFactorCurve.Evaluate(airDownTime);
			airDownTime += Time.fixedDeltaTime;
			airDownTime = Mathf.Min(AirDownFactorCurve.keys[AirDownFactorCurve.length - 1].time, airDownTime);
		}
		else if (airDownTime > 0f)
		{
			result = AirDownFactorCurve.Evaluate(airDownTime);
			airDownTime -= Time.fixedDeltaTime;
			airDownTime = Mathf.Max(0f, airDownTime);
		}
		return result;
	}

	public void UpdateCamera()
	{
		if (state == null)
		{
			return;
		}
		if (null == state.transform)
		{
			state = null;
			return;
		}
		dampRotVelocity();
		UpdateCameraFov();
		Vector3 position = state.rigidbody.position;
		float num = height + UpdateCameraAirDownHeigh();
		Quaternion quaternion = state.transform.localRotation;
		if (UseVelocityDir)
		{
			if (state.velocity.sqrMagnitude > 1f && state.LastNormal.sqrMagnitude > 0.9f)
			{
				quaternion = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(state.LastNormal, state.velocity), state.LastNormal), state.LastNormal);
			}
		}
		else if (state.LastNormal.sqrMagnitude >= 0.9f)
		{
			quaternion = Quaternion.LookRotation(state.rigidbody.rotation * vf, state.LastNormal);
		}
		if (Freeview)
		{
			quaternion *= ExRotation;
		}
		float deltaTime = Time.deltaTime;
		Vector3 defaultN = Vector3.up;
		float heightOffset = 0f;
		getNormalAndHeight(ref defaultN, ref heightOffset);
		if (dampRotation)
		{
			Vector3 eulerAngles = currentRotation.eulerAngles;
			Vector3 eulerAngles2 = quaternion.eulerAngles;
			float t = Mathf.Clamp01(curRotDamp * deltaTime);
			for (int i = 0; i < 3; i++)
			{
				eulerAngles[i] = Mathf.LerpAngle(eulerAngles[i], eulerAngles2[i], t);
			}
			for (int j = 0; j < 3; j++)
			{
				lastNormal[j] = Mathf.Lerp(lastNormal[j], defaultN[j], t);
			}
			lastHightOffset = Mathf.Lerp(lastHightOffset, heightOffset, t);
			currentRotation = Quaternion.Euler(eulerAngles);
		}
		else
		{
			lastNormal = defaultN;
			lastHightOffset = heightOffset;
			currentRotation = quaternion;
		}
		float num2 = distance;
		Vector3 vector = -Vector3.forward * num2;
		vector.y = num + lastHightOffset;
		vector = currentRotation * vector + position;
		base.transform.position = Vector3.Lerp(base.transform.position, vector, PosDamping);
		if (look)
		{
			base.transform.LookAt(position, lastNormal);
		}
	}

	private void getNormalAndHeight(ref Vector3 defaultN, ref float heightOffset)
	{
		if (state.ClosestNode == null)
		{
			return;
		}
		Vector3 position = state.rigidbody.position;
		Vector3 forward = state.ClosestNode.Forward;
		Vector3 rhs = position - state.ClosestNode.transform.position;
		RacePathNode racePathNode = ((Vector3.Dot(forward, rhs) > 0f) ? state.ClosestNode : state.ClosestNode.ParentNode);
		if (racePathNode != null && racePathNode.LeftNode != null)
		{
			forward = racePathNode.LeftNode.transform.position - racePathNode.transform.position;
			if (forward.sqrMagnitude < 0.001f)
			{
				defaultN = (racePathNode.useTransformNormal ? racePathNode.transform.up : (racePathNode.CameraAngle * Vector3.up));
			}
			rhs = position - racePathNode.transform.position;
			float t = Mathf.Clamp01(Mathf.Sqrt(Vector3.Project(rhs, forward).sqrMagnitude / forward.sqrMagnitude));
			defaultN = Vector3.Slerp(racePathNode.useTransformNormal ? racePathNode.transform.up : (racePathNode.CameraAngle * Vector3.up), racePathNode.LeftNode.useTransformNormal ? racePathNode.LeftNode.transform.up : (racePathNode.LeftNode.CameraAngle * Vector3.up), t);
			heightOffset = Mathf.Lerp(racePathNode.CameraHightOffset, racePathNode.LeftNode.CameraHightOffset, t);
		}
	}

	private Quaternion getUpDownRotation()
	{
		float num = 90f;
		if (state.LinearVelocity > 1f || state.LinearVelocity < -1f)
		{
			num = Vector3.Angle(Vector3.up, state.velocity);
		}
		float num2 = 0f;
		if (num - 90f > 0f)
		{
			num = Mathf.Min(num - 90f, 60f);
			num2 = 20f * ((0f - num) / 60f);
		}
		else
		{
			num = Mathf.Max(num - 90f, -60f);
			num2 = 20f * ((0f - num) / 60f);
		}
		return Quaternion.Euler(num2, 0f, 0f);
	}

	public Vector3 GetTargetPos(CarState car, Vector3 normal)
	{
		if (car == null || car.rigidbody == null)
		{
			return new Vector3(0f, 0f, 0f);
		}
		Quaternion quaternion = car.transform.localRotation;
		if (UseVelocityDir)
		{
			if (normal.sqrMagnitude > 0.9f)
			{
				quaternion = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(normal, car.transform.forward), normal), normal);
			}
		}
		else if (normal.sqrMagnitude >= 0.9f)
		{
			quaternion = Quaternion.LookRotation(car.rigidbody.rotation * vf, normal);
		}
		Vector3 vector = -Vector3.forward * distance;
		vector.y = height;
		return quaternion * vector + car.rigidbody.position;
	}

	public void ShakeCamera(ShakeType st = ShakeType.X, float duration = 0.25f)
	{
		if (viewCamera == null || shaking)
		{
			return;
		}
		Transform ct = viewCamera.transform;
		float startTime = Time.time;
		Quaternion[] offsets = xShakeOffset;
		switch (st)
		{
		case ShakeType.Y:
			offsets = yShakeOffset;
			break;
		case ShakeType.XnY:
			offsets = xyShakeOffset;
			break;
		case ShakeType.Random:
			offsets = rShakeOffset;
			break;
		case ShakeType.YBack:
			offsets = yBackOffset;
			break;
		}
		Quaternion oldPos = ct.localRotation;
		int index = Time.frameCount;
		Action acUpdate = null;
		acUpdate = delegate
		{
			if (ct == null || Time.time - startTime > duration)
			{
				AbstractView view2 = RaceCallback.View;
				view2.OnLateUpdate = (Action)Delegate.Remove(view2.OnLateUpdate, acUpdate);
				if (ct != null)
				{
					ct.localRotation = oldPos;
				}
				shaking = false;
			}
			else
			{
				index++;
				ct.localRotation = Quaternion.Lerp(oldPos, offsets[index % offsets.Length], Mathf.Clamp01((duration - Time.time + startTime) / duration));
			}
		};
		AbstractView view = RaceCallback.View;
		view.OnLateUpdate = (Action)Delegate.Combine(view.OnLateUpdate, acUpdate);
		shaking = true;
	}

	internal void ApplyLeaning()
	{
		if (leanState != 0 && !(ViewCamera == null))
		{
			Transform transform = ViewCamera.transform;
			float num = Mathf.Max(0f, (Time.time - startTime) / timeFactor);
			if (num >= 1f)
			{
				transform.localRotation = target;
				leanState = LeanType.None;
			}
			else
			{
				transform.localRotation = Quaternion.Lerp(startAt, target, interpolate(num));
			}
		}
	}

	internal void StartLeanTo(float steer, float speedFactor = 2.5f)
	{
		if (!(viewCamera == null))
		{
			if (steer == 0f)
			{
				leanState = LeanType.Reset;
				target = identity;
				cur = BackCurve;
			}
			else if (steer < 0f)
			{
				leanState = LeanType.Left;
				target = RaceConfig.CamLeftLean;
				cur = LeanCurve;
			}
			else
			{
				leanState = LeanType.Right;
				target = RaceConfig.CamRightLean;
				cur = LeanCurve;
			}
			startTime = Time.time;
			startAt = viewCamera.transform.localRotation;
			timeFactor = Mathf.Abs(Quaternion.Angle(startAt, target)) / 30f * speedFactor;
			if (timeFactor == 0f)
			{
				timeFactor = 0.1f;
			}
		}
	}

	internal void LeanEnd(float speedFactor = 0.01f)
	{
		leanState = LeanType.None;
	}

	private float interpolate(float t)
	{
		if (cur == null)
		{
			return 1f;
		}
		return cur.Evaluate(t);
	}
}
