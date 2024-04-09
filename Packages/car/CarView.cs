using System;
using System.Collections.Generic;
using CarLogic;
using UnityEngine;

public class CarView : AbstractView
{
	public static Action<CarView> InitCarView = null;

	public CarInfo PlayerInfo = new CarInfo();

	public bool AutoWheelCollider = true;

	public CarColorBundle ColorBundle;

	[SerializeField]
	protected int carId;

	public CarModel carModel = new CarModel();

	public CarState carState = new CarState();

	private RunState runState;

	public RunState LastRunState;

	public CarController Controller = new CarController();

	public SyncMoveController SyncController = new SyncMoveController();

	public CollisionController crasherController = new CollisionController();

	public CarShakeController ShakeController = new CarShakeController();

	public CarCenterOfMassController CenterOfMassController = new CarCenterOfMassController();

	internal PathController pathController = new PathController();

	internal CarEffectController carEffectController = new CarEffectController();

	internal ItemController ItController = new ItemController();

	internal SkidmarkController SkController = new SkidmarkController();

	internal CarAudioController AudioController = new CarAudioController();

	internal CarFlipController carFlipper;

	internal ResetController Reseter = new ResetController();

	internal AnimationController AnimController = new AnimationController();

	internal PathAnimationController PathAnimController = new PathAnimationController();

	public AICarController Ai = new AICarController();

	internal WeatherController weather;

	public N2ScrapeController N2Scraper = new N2ScrapeController();

	internal CarSkillController SkillController = new CarSkillController();

	public GameObject ShadowPrefab;

	[HideInInspector]
	public MiniMapMark MapMarkController;

	private float wheelRadius = 0.4f;

	private RespawnController _respawnController = new RespawnController();

	[HideInInspector]
	public Transform Human;

	public Material CarMaterial;

	private Material[] carMaterials;

	public Transform[] frontWheels;

	public Transform[] frontGraphics;

	public Transform[] rearWheels;

	public Transform[] rearGraphics;

	public Collider[] BodyColliders;

	public Material[] CharMaterials;

	[HideInInspector]
	public Transform[] ExhaustLocations;

	[HideInInspector]
	public Transform[] N2GasLocations;

	public Renderer[] CarRenders;

	[HideInInspector]
	public Transform HumanModel;

	private WheelFrictionCurve wfc;

	private WheelFrictionCurve fwfc;

	private float initialDragMultiplierX = 10f;

	[HideInInspector]
	public AudioSource ExSource;

	[HideInInspector]
	public AudioSource ExEffectSource;

	[HideInInspector]
	public EffectAnchor[] CarLightAnchors;

	[HideInInspector]
	public EffectAnchor[] RearLampAnchors;

	[HideInInspector]
	public EffectAnchor[] WheelEffectAnchors;

	[HideInInspector]
	public EffectAnchor[] SmallN2GasAnchors;

	[HideInInspector]
	public EffectAnchor[] BigN2GasAnchors;

	[HideInInspector]
	public EffectAnchor[] SkidmarkAnchors;

	[HideInInspector]
	public Transform PathNodeCollider;

	private bool unlit = true;

	private float totalFixDeltaTime;

	public object UserData;

	[HideInInspector]
	public bool InvalidRoad;

	[HideInInspector]
	private Transform models;

	[HideInInspector]
	private List<Transform> shakeEffects = new List<Transform>();

	public Action<ControllerColliderHit> OnControllerHit;

	public Action<Collider> OnWallDrag;

	public Action<bool> OnVisibleChange;

	public Action AcOnDrawGizmosSelected;

	private Quaternion qZero = Quaternion.identity;

	private Quaternion leftLean = Quaternion.Euler(0f, -35f, 0f);

	private Quaternion rightLean = Quaternion.Euler(0f, 35f, 0f);

	private Quaternion lastLean = Quaternion.identity;

	private Quaternion qt1 = Quaternion.identity;

	private GameObject dropdownEffect;

	private float lastSteer;

	private Vector3 vt1;

	private Vector3 vt2;

	private Vector3 vup = Vector3.up;

	internal int waitDropdown;

	private RaceItemBase demoItem;

	public bool IsDemo;

	[HideInInspector]
	public bool autoMove;

	private Vector3 dir = Vector3.left;

	private int wheelCount;

	private Vector3 wheelPos;

	private static Vector3 constRight = Vector3.right;

	private float rotAngle;

	private float flashStartTime;

	private float finalAngle;

	private float angleTime;

	private const int flashTime = 3;

	private const float flashInterval = 0.6f;

	private const float minAlpha = 0.35f;

	private Transform tranN2Gas;

	public PlayerType CarPlayerType
	{
		get
		{
			return carState.CarPlayerType;
		}
		set
		{
			carState.CarPlayerType = value;
		}
	}

	public int CarId
	{
		get
		{
			return carId;
		}
		set
		{
			carId = value;
		}
	}

	public RunState RunState
	{
		get
		{
			return runState;
		}
		set
		{
			LastRunState = runState;
			runState = value;
		}
	}

	public bool Unlit
	{
		get
		{
			return unlit;
		}
		set
		{
			unlit = value;
			ResetShader();
		}
	}

	public float TotalFixedDeltaTime => totalFixDeltaTime;

	public Color CarColor
	{
		get
		{
			if (carMaterials == null || carMaterials.Length == 0)
			{
				return Color.white;
			}
			return carMaterials[0].color;
		}
		set
		{
			setColor(value);
		}
	}

	public float LapPastDistance
	{
		get
		{
			if (pathController != null)
			{
				return pathController.LapPastDistance;
			}
			return 0f;
		}
		set
		{
			if (pathController != null)
			{
				pathController.LapPastDistance = value;
			}
		}
	}

	public Transform CarBody => models;

	public List<Transform> ShakeEffects => shakeEffects;

	public bool SyncEnable
	{
		get
		{
			return SyncController.Active;
		}
		set
		{
			SyncController.Active = value;
		}
	}

	public bool DriftEnable
	{
		get
		{
			return Controller.Drifter.Active;
		}
		set
		{
			Controller.Drifter.Active = value;
		}
	}

	public bool AiEnable
	{
		get
		{
			return Ai.Active;
		}
		set
		{
			Ai.Active = value;
		}
	}

	public bool SelfControlEnable
	{
		get
		{
			return Controller.Active;
		}
		set
		{
			Controller.Active = value;
		}
	}

	public bool Resetable
	{
		get
		{
			return Reseter.Active;
		}
		set
		{
			Reseter.Active = value;
		}
	}

	public EffectToggleBase RushEffect
	{
		get
		{
			return carEffectController.RushEffect;
		}
		set
		{
			carEffectController.RushEffect = value;
		}
	}

	public EffectToggleBase SimpleGas
	{
		get
		{
			return carEffectController.simpleGas;
		}
		set
		{
			carEffectController.simpleGas = value;
		}
	}

	public EffectToggleBase SimpleN2Effect
	{
		get
		{
			return carEffectController.SimpleN2Effect;
		}
		set
		{
			carEffectController.SimpleN2Effect = value;
		}
	}

	public EffectToggleBase TranslateEffect
	{
		get
		{
			return carEffectController.TranslateEffect;
		}
		set
		{
			carEffectController.TranslateEffect = value;
		}
	}

	public EffectToggleBase RearLampEffect
	{
		get
		{
			return carEffectController.RearLampEffect;
		}
		set
		{
			carEffectController.RearLampEffect = value;
		}
	}

	public CarEffectController.AirShipEffectGroup AirShipEffects
	{
		get
		{
			return carEffectController.AirShipEffects;
		}
		set
		{
			carEffectController.AirShipEffects = value;
		}
	}

	public Transform ExExhaust
	{
		get
		{
			return carEffectController.ExExhaust;
		}
		set
		{
			carEffectController.ExExhaust = value;
		}
	}

	public Transform ExGas
	{
		get
		{
			return carEffectController.ExGas;
		}
		set
		{
			carEffectController.ExGas = value;
		}
	}

	public Transform ExGasAirflow
	{
		get
		{
			return carEffectController.ExGasAirflow;
		}
		set
		{
			carEffectController.ExGasAirflow = value;
		}
	}

	public Transform ExYGasAirflow
	{
		get
		{
			return carEffectController.ExYGasAirflow;
		}
		set
		{
			carEffectController.ExYGasAirflow = value;
		}
	}

	public Transform ExPGasAirflow
	{
		get
		{
			return carEffectController.ExPGasAirflow;
		}
		set
		{
			carEffectController.ExPGasAirflow = value;
		}
	}

	public Transform ExCupidGas
	{
		get
		{
			return carEffectController.ExCupidGas;
		}
		set
		{
			carEffectController.ExCupidGas = value;
		}
	}

	public EffectToggleBase GasToggle
	{
		get
		{
			return carEffectController.GasToggle;
		}
		set
		{
			carEffectController.GasToggle = value;
		}
	}

	public EffectToggleBase HightSpeedToggle
	{
		get
		{
			return carEffectController.HightSpeedToggle;
		}
		set
		{
			carEffectController.HightSpeedToggle = value;
		}
	}

	public TeamGasScraper AiTeamGas => Ai.TeamGas;

	public int AiMapId => Ai.MapId;

	public int AiTrackId => Ai.TrackId;

	public uint AiItemStrategy => Ai.ItemStrategy;

	public int AiRecordCount => Ai.RecordCount;

	public float AiMoveStartTime => Ai.MoveStartTime;

	public float AiPastTime => Ai.PastTime;

	public List<ItemToggle> AiToggles => Ai.Toggles;

	public bool AiPaused
	{
		get
		{
			return Ai.Paused;
		}
		set
		{
			Ai.Paused = value;
		}
	}

	public float AsyncInterval => SyncController.AsyncInterval;

	public bool SkidEffectOn
	{
		get
		{
			if (SkController != null)
			{
				return SkController.Active;
			}
			return false;
		}
		set
		{
			if (SkController != null)
			{
				SkController.Active = value;
			}
		}
	}

	public bool RaycastSkidOn
	{
		get
		{
			if (SkController != null)
			{
				return SkController.IsRaycastSkid;
			}
			return false;
		}
		set
		{
			if (SkController != null)
			{
				SkController.IsRaycastSkid = value;
			}
		}
	}

	public void SetUpCarRenders(Renderer[] render)
	{
		int num = render.Length;
		for (int i = 0; i < num; i++)
		{
			if (null != render[i])
			{
				CarRenders[i] = render[i];
			}
		}
	}

	public void SetCheckLapCountCorrect(bool value)
	{
		if (pathController != null)
		{
			pathController.CheckLapCountCorrect = value;
		}
	}

	public override void Awake()
	{
		if (InitCarView != null)
		{
			InitCarView(this);
		}
		if (CarLightAnchors == null)
		{
			CarLightAnchors = new EffectAnchor[0];
		}
		if (RearLampAnchors == null)
		{
			RearLampAnchors = new EffectAnchor[0];
		}
		if (SmallN2GasAnchors == null)
		{
			SmallN2GasAnchors = new EffectAnchor[0];
		}
		if (BigN2GasAnchors == null)
		{
			BigN2GasAnchors = new EffectAnchor[0];
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (!IsDemo)
		{
			IsDemo = Application.loadedLevelName.ToLower().Contains("demo");
		}
		carState.transform = base.transform;
		carState.rigidbody = GetComponent<Rigidbody>();
		carState.view = this;
		ExSource = base.gameObject.AddComponent<AudioSource>();
		ExSource.minDistance = 4f;
		ExEffectSource = base.gameObject.AddComponent<AudioSource>();
		ExEffectSource.minDistance = 4f;
		Controller.Init(this, base.transform, carState, carModel);
		models = base.transform.Find("Models");
		if (carModel.carModelType != 0 && shakeEffects != null)
		{
			Transform transform = base.transform.Find("Effects/feichuan");
			if (transform != null)
			{
				shakeEffects.Add(transform);
			}
		}
		carFlipper = new CarFlipController(carState);
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		Reseter.Init(carState, this);
		SkController.Init(this, carState);
		if (carEffectController == null)
		{
			carEffectController = new CarEffectController();
		}
		carEffectController.Init(this, carState);
		crasherController.Init(this, carState);
		pathController.Init(this, carState);
		ItController.Init(carState, carModel);
		ShakeController.Init(carState);
		PathAnimController.Init(carState);
		SkillController.Init(carState);
		carState.AirGround.Init(carState);
		carState.CollisionState.Init(this, carState);
		Ai.Init(this, carState);
		Ai.Active = false;
		AudioController.Init(carState);
		AudioController.Active = true;
		N2Scraper.Init(carState, carModel);
		N2Scraper.Active = false;
		carState.Wheels = new Wheel[frontWheels.Length + rearWheels.Length];
		carState.HitLenghts = new float[frontWheels.Length + rearWheels.Length];
		if (CarRenders != null && CarRenders.Length != 0)
		{
			VisibleDelegate visibleDelegate = CarRenders[0].gameObject.AddComponent<VisibleDelegate>();
			visibleDelegate.AcOnVisibleChange = onVisibleChange;
		}
		base.Awake();
	}

	public override void Start()
	{
		AnimController.Init(this, carState, carModel.carModelType);
		AnimController.Active = true;
		setupCarMaterial(this);
		Controller.Init();
		if (BodyColliders != null && BodyColliders.Length != 0)
		{
			for (int i = 0; i < BodyColliders.Length; i++)
			{
				if (BodyColliders[i] != null)
				{
					BodyColliders[i].gameObject.layer = 14;
				}
			}
			if (CarPlayerType != 0)
			{
				BoxCollider boxCollider = BodyColliders[0] as BoxCollider;
				if ((bool)boxCollider && CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					boxCollider.isTrigger = !SyncMoveController.OpenNewCollision;
				}
				carState.rigidbody.mass *= 2f;
			}
		}
		SetupWheelColliders();
		carState.rigidbody.isKinematic = false;
		carState.rigidbody.useGravity = false;
		if (CarPlayerType == PlayerType.PLAYER_SELF)
		{
			ApplyParams(carModel, base.transform, GetComponent<Rigidbody>());
			carState.rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Reseter.AddChecker(pathController);
			Reseter.AddChecker(Controller);
			Reseter.AddChecker(carFlipper);
			_respawnController.SetCarState(carState);
			_respawnController.Active = true;
			Reseter.AddChecker(_respawnController);
			weather = new WeatherController();
			weather.Init(this);
		}
		else if (CarPlayerType == PlayerType.PLAYER_OTHER)
		{
			carState.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			Debug.LogError("Modify by Sbin: CarView.Start: dont SetConstraints");
		}
		else
		{
			carState.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			carState.rigidbody.freezeRotation = true;
			RigidbodyTool.AppendConstraints(carState.view, (RigidbodyConstraints)10);
		}
		carState.CarPlayerType = CarPlayerType;
		SyncController.Init(this, carState, carModel);
		Controller.Active = CarPlayerType == PlayerType.PLAYER_SELF;
		crasherController.Active = true;
		N2Scraper.Active = CarPlayerType == PlayerType.PLAYER_SELF;
		carFlipper.Active = CarPlayerType == PlayerType.PLAYER_SELF;
		SkController.Active = carModel.carModelType == CarModelType.Car && (!RaceConfig.SelfSkidmarkOnly || CarPlayerType == PlayerType.PLAYER_SELF);
		ShakeController.Active = CarPlayerType == PlayerType.PLAYER_SELF;
		SkillController.Active = CarPlayerType == PlayerType.PLAYER_SELF;
		PathAnimController.Active = true;
		Reseter.Active = true;
		ItController.Active = true;
		pathController.Active = true;
		carEffectController.Active = true;
		initialDragMultiplierX = carModel.DragMultiplier.x;
		if (CarPlayerType == PlayerType.PLAYER_SELF || CarPlayerType == PlayerType.PLAYER_AUTO_AI)
		{
			setupBalancer();
			if ((bool)CameraController.Current && CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CameraController.Current.Init(this, carState);
			}
		}
		ResetShader();
		base.Start();
	}

	public void SetAsAutoAI()
	{
		Controller.Active = true;
		pathController.Active = true;
		carFlipper.Active = true;
		Ai.Active = false;
		Reseter.AddChecker(pathController);
		Reseter.AddChecker(Controller);
		Reseter.AddChecker(carFlipper);
		_respawnController.SetCarState(carState);
		_respawnController.Active = true;
		Reseter.AddChecker(_respawnController);
		ApplyParams(carModel, base.transform, GetComponent<Rigidbody>());
		carState.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		carState.rigidbody.freezeRotation = false;
		setupBalancer();
	}

	public void SetIgnoreItemBox(bool value)
	{
		if (ItController != null)
		{
			ItController.IgnoreItemBox = value;
		}
	}

	public void SetBoxLock(bool value)
	{
		if (ItController != null)
		{
			ItController.BoxLock = value;
		}
	}

	public void SetAsAI()
	{
		CarPlayerType = PlayerType.PALYER_AI;
		Ai.Active = true;
	}

	public void SetUpGasEffect(Transform transformGas = null)
	{
		if ((bool)transformGas)
		{
			transformGas.localRotation = Quaternion.identity;
			carEffectController.ExGas = transformGas;
		}
		carEffectController.Init(this, carState);
	}

	public void SetUpGasAirflowEffect(Transform transformGasAirflow = null, int index = 0)
	{
		if ((bool)transformGasAirflow)
		{
			transformGasAirflow.localRotation = Quaternion.identity;
			switch (index)
			{
			case 0:
				carEffectController.ExGasAirflow = transformGasAirflow;
				break;
			case 1:
				carEffectController.ExYGasAirflow = transformGasAirflow;
				break;
			case 2:
				carEffectController.ExPGasAirflow = transformGasAirflow;
				break;
			}
		}
		carEffectController.Init(this, carState);
	}

	public void SetUpCoupleGasEffect(Transform gasNode = null)
	{
		if ((bool)gasNode)
		{
			gasNode.localRotation = Quaternion.identity;
			carEffectController.ExCupidGas = gasNode;
		}
		carEffectController.Init(this, carState);
	}

	public void UpdateEffectController()
	{
		if (carEffectController != null)
		{
			carEffectController.UpdateEffect();
		}
	}

	private void setupShadow()
	{
		if ((bool)ShadowPrefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((UnityEngine.Object)ShadowPrefab) as GameObject;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = new Vector3(0f, -0.05f, 0f);
		}
	}

	public override void Update()
	{
		base.Update();
		if (CarPlayerType == PlayerType.PLAYER_SELF && IsDemo)
		{
			GetInput();
		}
		if (carState.Visible)
		{
			if (carState.TransQTEok == 0)
			{
				UpdateWheelGraphics(carState.relativeVelocity);
			}
			updateModels();
			if (waitDropdown > 0)
			{
				updateDropDownEffect();
			}
		}
		totalFixDeltaTime = 0f;
	}

	public override void FixedUpdate()
	{
		PlayerType carPlayerType = CarPlayerType;
		totalFixDeltaTime += Time.deltaTime;
		updateCommonState();
		base.FixedUpdate();
		carState.LastPosition = carState.rigidbody.position;
		carState.LastRotation = carState.rigidbody.rotation;
		if (CarPlayerType == PlayerType.PLAYER_SELF)
		{
			carState.LastVelocity = carState.rigidbody.velocity;
			DoUpdateRot();
		}
		crasherController.FixedUpdate();
	}

	private void updateModels()
	{
		if (!(models == null) && CarPlayerType == PlayerType.PLAYER_SELF && carState.rigidbody.collisionDetectionMode == CollisionDetectionMode.Continuous)
		{
			float num = Mathf.Clamp(Mathf.Abs(carState.relativeVelocity.x) - 5f, 0f, 30f);
			num = (0f - Mathf.Sign(carState.relativeVelocity.x)) * num;
			float value = num / Mathf.Clamp(carState.relativeVelocity.z, 5f, 30f);
			float num2 = 2f;
			models.localRotation = Quaternion.Slerp(RaceConfig.LeftLean, RaceConfig.RightLean, Mathf.Clamp01((Mathf.Clamp(value, 0f - num2, num2) / num2 + 1f) * 0.5f));
		}
	}

	public void ResetModels()
	{
		if (models != null)
		{
			models.localRotation = Quaternion.identity;
		}
	}

	private void updateCommonState()
	{
		carState.eularAngle = carState.transform.eulerAngles;
		carState.LastNormal = GetGNormalByNode(carState.rigidbody.position, autoMeshNormal: false);
	}

	private void SetupWheelColliders()
	{
		int num = 0;
		SetupWheelFrictionCurve(carModel.SideWayFriction);
		for (int i = 0; i < frontWheels.Length; i++)
		{
			carState.Wheels[num] = SetupWheel(frontWheels[i], isFrontWheel: true, i);
			num++;
		}
		for (int j = 0; j < rearWheels.Length; j++)
		{
			carState.Wheels[num] = SetupWheel(rearWheels[j], isFrontWheel: false, j);
			num++;
		}
	}

	private void setupBalancer()
	{
		Debug.LogWarning("Modify by Sbin in CarView:  setupBalancer()    don't use ConfigurableJoint ");
	}

	internal void SetupWheelFrictionCurve(float stiff = 0f)
	{
		wfc = default(WheelFrictionCurve);
		Debug.LogWarning(" Modify by Sbin in CarView : SetupWheelFrictionCurve ");
		wfc.extremumSlip = 0.5f;
		wfc.extremumValue = 1f;
		wfc.asymptoteSlip = 0.8f;
		wfc.asymptoteValue = 0.5f;
		wfc.stiffness = stiff;
		fwfc = wfc;
		fwfc.stiffness = 1f;
	}

	public void SetFrontGraphics(Transform[] frontGraph)
	{
		frontGraphics = frontGraph;
	}

	public void SetBackGraphic(Transform[] backGraph)
	{
		rearGraphics = backGraph;
	}

	private Wheel SetupWheel(Transform wheelTransform, bool isFrontWheel, int i)
	{
		float x = base.transform.lossyScale.x;
		GameObject gameObject = wheelTransform.gameObject;
		int num = LayerMask.NameToLayer("Wheel");
		if (num != -1)
		{
			gameObject.layer = num;
		}
		WheelCollider wheelCollider = gameObject.GetComponent<WheelCollider>();
		if (wheelCollider == null)
		{
			wheelCollider = gameObject.AddComponent(typeof(WheelCollider)) as WheelCollider;
		}
		if (AutoWheelCollider)
		{
			wheelCollider.suspensionDistance = (float)carModel.suspensionRange * x;
			JointSpring suspensionSpring = wheelCollider.suspensionSpring;
			if (isFrontWheel)
			{
				suspensionSpring.spring = carModel.suspensionSpringFront;
			}
			else
			{
				suspensionSpring.spring = carModel.suspensionSpringRear;
			}
			suspensionSpring.damper = carModel.suspensionDamper;
			wheelCollider.suspensionSpring = suspensionSpring;
		}
		else
		{
			if (isFrontWheel)
			{
				carModel.suspensionSpringFront = wheelCollider.suspensionSpring.spring;
			}
			else
			{
				carModel.suspensionSpringRear = wheelCollider.suspensionSpring.spring;
			}
			carModel.suspensionRange = wheelCollider.suspensionDistance;
		}
		Wheel wheel = new Wheel();
		wheel.collider = wheelCollider;
		wheelCollider.sidewaysFriction = wfc;
		wheelCollider.forwardFriction = fwfc;
		wheel.tireGraphic = (isFrontWheel ? frontGraphics[i] : rearGraphics[i]);
		if (wheel.tireGraphic != null)
		{
			gameObject = new GameObject(wheelTransform.name + " Column");
			Transform transform = gameObject.transform;
			transform.position = wheel.tireGraphic.position;
			transform.rotation = wheel.tireGraphic.rotation;
			transform.parent = base.transform;
			wheel.tireGraphic.parent = transform;
			wheel.tireGraphic.localPosition = Vector3.zero;
			MeshFilter component = wheel.tireGraphic.GetComponent<MeshFilter>();
			if (component != null && component.sharedMesh != null)
			{
				if (isFrontWheel)
				{
					carModel.FrontWheelRadius = component.sharedMesh.bounds.size.z / 2f;
				}
				else
				{
					carModel.RearWheelRadius = component.sharedMesh.bounds.size.z / 2f;
				}
			}
			wheel.startY = base.transform.InverseTransformPoint(wheelCollider.transform.position).y - base.transform.InverseTransformPoint(transform.position).y + transform.localPosition.y;
		}
		if (isFrontWheel)
		{
			wheel.steerWheel = true;
		}
		else
		{
			wheel.driveWheel = true;
		}
		return wheel;
	}

	public void GetInput()
	{
		ControlType controlType = RaceCallback.GetControlType();
		if (controlType == ControlType.TouchType || controlType == ControlType.Gravity)
		{
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.OSXPlayer)
			{
				carState.Throttle = (autoMove ? 1 : 0);
				carState.Steer = 0f;
				if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
				{
					carState.Throttle = 1f;
				}
				else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
				{
					carState.Throttle = -1f;
				}
				if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				{
					carState.Steer = 1f;
				}
				else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				{
					carState.Steer = -1f;
				}
				carState.N2Force = Input.GetKey(KeyCode.F);
				carState.Drift = Input.GetKey(KeyCode.LeftShift);
				carState.HandBrake = Input.GetKey(KeyCode.Space);
				if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
				{
					bool flag = false;
					flag |= carState.AirGround.TryStartGas();
					if (flag | carState.view.Controller.TryStartSecondSmallGas())
					{
						return;
					}
				}
			}
			else
			{
				RaceCallback.GetInput(carState);
			}
		}
		CheckSteer();
		if (!IsDemo)
		{
			return;
		}
		if (carState.Throttle == 0f && autoMove)
		{
			carState.Throttle = 1f;
		}
		if (!carState.N2Force || demoItem != null)
		{
			return;
		}
		demoItem = new CommonGasItem(1, carState.view.carModel.N2ForceTime, useShake: false);
		ItemParams itemParams = new ItemParams(null, null, 0);
		itemParams.targets = new CarState[1] { carState };
		itemParams.user = carState;
		if (demoItem.Usable(itemParams))
		{
			((CommonGasItem)demoItem).BreakCallback = delegate
			{
				demoItem = null;
			};
			demoItem.Toggle(itemParams);
		}
		carState.N2Force = false;
	}

	public void CheckSteer()
	{
		if (carState.Steer == lastSteer)
		{
			return;
		}
		carState.SteerChangeTime = Time.time;
		lastSteer = carState.Steer;
		if (carState.Wheels != null && carState.Wheels.Length != 0)
		{
			Wheel wheel = carState.Wheels[0];
			if (wheel != null && wheel.tireGraphic != null)
			{
				lastLean = wheel.tireGraphic.parent.localRotation;
			}
		}
	}

	public void UpdateWheelGraphics(Vector3 relativeVelocity)
	{
		if (carState == null || carState.Wheels == null || carModel == null)
		{
			return;
		}
		wheelCount = -1;
		if (carState.WheelHits == null)
		{
			carState.WheelHits = new WheelHit[carState.Wheels.Length];
		}
		float num = relativeVelocity.z / 5f;
		rotAngle += num * Time.deltaTime / (float)carModel.FrontWheelRadius;
		rotAngle = Mathf.Repeat(rotAngle, (float)Math.PI * 40f);
		Quaternion localRotation = Quaternion.AngleAxis(rotAngle * 57.29578f, constRight);
		if (carState.Steer == 0f)
		{
			qt1 = qZero;
		}
		else if (carState.Steer < 0f)
		{
			qt1 = leftLean;
		}
		else
		{
			qt1 = rightLean;
		}
		qt1 = Quaternion.Lerp(lastLean, qt1, Mathf.Clamp01(Time.time - carState.SteerChangeTime) / (float)carModel.FullRotateTime * 5f);
		for (int i = 0; i < carState.Wheels.Length; i++)
		{
			Wheel wheel = carState.Wheels[i];
			wheelCount++;
			if (wheel == null || wheel.collider == null)
			{
				continue;
			}
			WheelCollider wheelCollider = wheel.collider;
			wheelPos.Set(0f, 0f, 0f);
			if ((bool)wheel.tireGraphic)
			{
				if ((carState.WheelHitState & (1 << wheelCount)) != 0)
				{
					wheel.tireGraphic.localRotation = localRotation;
				}
				Transform parent = wheel.tireGraphic.transform.parent;
				vt1 = parent.localPosition;
				vt1.y = 0f - (carState.HitLenghts[wheelCount] + 0.5f * (wheelCollider.radius + wheelCollider.suspensionDistance) - carState.Wheels[wheelCount].startY - (float)((wheelCount < 2) ? carModel.FrontWheelRadius : carModel.RearWheelRadius));
				if (float.IsNaN(vt1.y))
				{
					vt1.y = 0f;
				}
				Debug.LogWarning("  Modify by Sbin in CarView : UpdateWheelGraphics ");
				Vector3 pos = Vector3.zero;
				Quaternion quat = Quaternion.identity;
				if (wheel.collider.enabled)
				{
					wheel.collider.GetWorldPose(out pos, out quat);
					parent.position = pos;
					parent.rotation = quat;
				}
			}
		}
	}

	internal void updateWheelDownForce(float gravity = 50f)
	{
		if (!(carState.rigidbody != null) || carState.rigidbody.isKinematic)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			int groundHit = carState.GroundHit;
			Wheel wheel = carState.Wheels[i];
			if (groundHit != 0 && wheel != null && (groundHit & (1 << i)) == 0)
			{
				vt1 = (0f - (float)carModel.CarWeight) * 50f * carState.GroundNormal;
				carState.rigidbody.AddForceAtPosition(vt1, wheel.collider.transform.position);
			}
		}
	}

	internal Vector3 GetTransformUpByNodeAuto()
	{
		if (carState == null)
		{
			return Vector3.up;
		}
		if (carState.ClosestNode == null)
		{
			return Vector3.up;
		}
		Vector3 position = carState.rigidbody.position;
		vt1 = carState.ClosestNode.Forward;
		vt2 = position - carState.ClosestNode.transform.position;
		RacePathNode racePathNode = ((Vector3.Dot(vt1, vt2) > 0f) ? carState.ClosestNode : carState.ClosestNode.ParentNode);
		if (racePathNode == null)
		{
			return carState.ClosestNode.transform.up;
		}
		if (racePathNode.LeftNode == null)
		{
			return racePathNode.transform.up;
		}
		vt1 = racePathNode.LeftNode.transform.position - racePathNode.transform.position;
		if (vt1.sqrMagnitude < 0.001f)
		{
			return racePathNode.transform.up;
		}
		vt2 = position - racePathNode.transform.position;
		return Vector3.Slerp(racePathNode.transform.up, racePathNode.LeftNode.transform.up, Mathf.Clamp01(Mathf.Sqrt(Vector3.Project(vt2, vt1).sqrMagnitude / vt1.sqrMagnitude)));
	}

	internal Vector3 GetGNormalByNode(Vector3 pos, bool autoMeshNormal = true)
	{
		if (carState == null)
		{
			return Vector3.up;
		}
		if (carState.ClosestNode != null)
		{
			vt1 = carState.ClosestNode.Forward;
			vt2 = pos - carState.ClosestNode.transform.position;
			RacePathNode racePathNode = ((Vector3.Dot(vt1, vt2) > 0f) ? carState.ClosestNode : carState.ClosestNode.ParentNode);
			if (racePathNode != null)
			{
				if (racePathNode.LeftNode != null)
				{
					if (autoMeshNormal && carState.GroundHit != 0 && !racePathNode.IsTwist && !racePathNode.LeftNode.IsTwist)
					{
						return carState.GroundNormal;
					}
					vt1 = racePathNode.LeftNode.transform.position - racePathNode.transform.position;
					if (vt1.sqrMagnitude < 0.001f)
					{
						return racePathNode.Up;
					}
					vt2 = pos - racePathNode.transform.position;
					return Vector3.Slerp(racePathNode.Up, racePathNode.LeftNode.Up, Mathf.Clamp01(Mathf.Sqrt(Vector3.Project(vt2, vt1).sqrMagnitude / vt1.sqrMagnitude)));
				}
				if (!autoMeshNormal || carState.GroundHit == 0 || racePathNode.IsTwist)
				{
					return racePathNode.Up;
				}
				return carState.GroundNormal;
			}
			if (!autoMeshNormal || carState.GroundHit == 0)
			{
				return carState.ClosestNode.Up;
			}
			return carState.GroundNormal;
		}
		vt1.Set(0f, 1f, 0f);
		return vt1;
	}

	internal Vector3 GetGNormalByNodeSimple()
	{
		if (carState.ClosestNode != null)
		{
			return carState.ClosestNode.Up;
		}
		vt1.Set(0f, 1f, 0f);
		return vt1;
	}

	internal Vector3 GetGNormalAuto()
	{
		if (carState.GroundHit == 0)
		{
			return GetGNormalByNode(carState.rigidbody.position);
		}
		return carState.GroundNormal;
	}

	internal float GetGroundedPercent()
	{
		for (int i = 0; i < carState.Wheels.Length; i++)
		{
			if (((1 << i) & carState.GroundHit) != 0)
			{
				return carState.WheelHits[i].force / carState.Wheels[i].collider.suspensionSpring.spring;
			}
		}
		return 0f;
	}

	private void updateDropDownEffect()
	{
		if (carState.GroundHit == 0)
		{
			return;
		}
		waitDropdown = 0;
		if (dropdownEffect != null)
		{
			dropdownEffect.SetActive(value: false);
			dropdownEffect.SetActive(value: true);
			return;
		}
		RacePathManager.LoadDependence(RacePathManager.ActiveInstance, 5, delegate(UnityEngine.Object o)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
			if (!(gameObject == null) && carState.view != null)
			{
				vt1 = carState.transform.position;
				Transform transform = gameObject.transform;
				transform.position = vt1;
				transform.parent = carState.transform;
				if (dropdownEffect != null && dropdownEffect.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
				else
				{
					gameObject.SetActive(value: false);
				}
				dropdownEffect = gameObject;
			}
		}, "Effects/Sence/");
	}

	public void ApplyParams(CarModel model, Transform t, Rigidbody r)
	{
		r.centerOfMass = model.centerOfMass;
		r.mass = model.CarWeight;
		r.angularDrag = model.AngularDrag;
		CenterOfMassController.SetRigibody(r);
	}

	public void ForceGround()
	{
		if (CarPlayerType != 0)
		{
			CharacterController component = GetComponent<CharacterController>();
			if ((bool)component && Physics.Raycast(base.transform.position + new Vector3(0f, 2f, 0f), Vector3.down, out var hitInfo, 100f, 256))
			{
				Vector3 point = hitInfo.point;
				point.y += 0f - component.center.y + component.radius + 0.05f;
				base.transform.position = point;
			}
		}
	}

	public void DisableMovement()
	{
		GetComponent<Rigidbody>().useGravity = true;
		if (CarPlayerType == PlayerType.PLAYER_SELF)
		{
			Debug.LogError("--- Modify by Sbin in CarView.cs : DisableMovement , RigidbodyConstraints.FreezeAll  ");
			RigidbodyTool.SetConstraints(this, RigidbodyConstraints.FreezeAll);
		}
		else if (CarPlayerType == PlayerType.PLAYER_OTHER)
		{
			RigidbodyTool.SetConstraints(this, (RigidbodyConstraints)10);
		}
		else if (CarPlayerType == PlayerType.PALYER_AI)
		{
			Ai.Active = false;
			RigidbodyTool.SetConstraints(this, (RigidbodyConstraints)10);
		}
		SyncController.Active = false;
		carEffectController.StopAllEffects();
		if (carState.CallBacks.OnEnableMovement != null)
		{
			carState.CallBacks.OnEnableMovement(obj: false);
		}
	}

	public void EnableMovement()
	{
		GetComponent<Rigidbody>().useGravity = false;
		if (CarPlayerType == PlayerType.PLAYER_SELF)
		{
			RigidbodyTool.SetConstraints(this, RigidbodyConstraints.None);
			Controller.Active = true;
			SyncController.Active = true;
		}
		else if (CarPlayerType == PlayerType.PLAYER_OTHER)
		{
			Debug.LogError("Modify by Sbin: CarView.EnableMovement: RemoveConstraints.FreezeAll");
			RigidbodyTool.RemoveConstraints(this, RigidbodyConstraints.FreezeAll);
			SyncController.Active = true;
		}
		else if (CarPlayerType == PlayerType.PALYER_AI)
		{
			RigidbodyTool.SetConstraints(this, RigidbodyConstraints.FreezeRotation);
			Ai.Active = true;
		}
		if (carState.CallBacks.OnEnableMovement != null)
		{
			carState.CallBacks.OnEnableMovement(obj: true);
		}
	}

	public void WaitForDropDown()
	{
		Controller.setWheelState(carState);
		Controller.applyBalance();
		waitDropdown++;
	}

	internal CharacterController AddCharacterController()
	{
		CharacterController characterController = GetComponent<CharacterController>();
		if (characterController == null)
		{
			characterController = base.gameObject.AddComponent<CharacterController>();
			characterController.height = 0f;
		}
		characterController.center = new Vector3(0f, characterController.radius + 0.05f, 0f);
		return characterController;
	}

	public void OnReachPathNode(RacePathNode node)
	{
		if (carState.CallBacks.OnPathNodeArrived != null)
		{
			carState.CallBacks.OnPathNodeArrived(node, arg2: true);
		}
	}

	public void OnLeavePathNode(RacePathNode node)
	{
		if (carState.CallBacks.OnPathNodeArrived != null)
		{
			carState.CallBacks.OnPathNodeArrived(node, arg2: false);
		}
	}

	private void onWallProtect(Collider c)
	{
		if (OnWallDrag != null)
		{
			OnWallDrag(c);
		}
	}

	private void onVisibleChange(bool visible)
	{
		if (Application.isPlaying)
		{
			if (carState != null)
			{
				carState.Visible = visible;
			}
			if (OnVisibleChange != null)
			{
				OnVisibleChange(visible);
			}
		}
	}

	internal bool EnableSidewayFriction()
	{
		carState.SidewayFrictionFlag--;
		if (carState.SidewayFrictionFlag <= 0)
		{
			carState.SidewayFrictionFlag = 0;
			SetSideWayFriction(carModel.SideWayFriction);
		}
		return carState.SidewayFrictionFlag == 0;
	}

	internal bool DisableSidewayFriction()
	{
		if (carState.SidewayFrictionFlag == 0)
		{
			SetSideWayFriction(0f);
		}
		carState.SidewayFrictionFlag++;
		return carState.SidewayFrictionFlag == 1;
	}

	internal void SetFrontSideWayFriction(float stiff)
	{
		if (carState.Wheels == null || carState.Wheels.Length == 0 || carState.Wheels[0] == null || !(carState.Wheels[0].collider != null))
		{
			return;
		}
		WheelFrictionCurve sidewaysFriction = carState.Wheels[0].collider.sidewaysFriction;
		sidewaysFriction.stiffness = stiff;
		WheelFrictionCurve forwardFriction = carState.Wheels[0].collider.forwardFriction;
		forwardFriction.stiffness = stiff;
		for (int i = 0; i < 2; i++)
		{
			if (carState.Wheels[i] != null)
			{
				WheelCollider wheelCollider = carState.Wheels[i].collider;
				if ((bool)wheelCollider)
				{
					wheelCollider.sidewaysFriction = sidewaysFriction;
					wheelCollider.forwardFriction = forwardFriction;
				}
			}
		}
	}

	internal void SetSideWayFriction(float stiff)
	{
		if (carState.Wheels == null || carState.Wheels.Length == 0 || carState.Wheels[0] == null || !(carState.Wheels[0].collider != null))
		{
			return;
		}
		WheelFrictionCurve sidewaysFriction = carState.Wheels[0].collider.sidewaysFriction;
		sidewaysFriction.stiffness = stiff;
		for (int i = 0; i < carState.Wheels.Length; i++)
		{
			if (carState.Wheels[i] != null)
			{
				WheelCollider wheelCollider = carState.Wheels[i].collider;
				if ((bool)wheelCollider)
				{
					wheelCollider.sidewaysFriction = sidewaysFriction;
				}
			}
		}
	}

	internal void setColor(Color color)
	{
		if (carMaterials == null)
		{
			setupCarMaterial(this);
		}
		if (carMaterials != null)
		{
			for (int i = 0; i < carMaterials.Length; i++)
			{
				carMaterials[i].color = color;
			}
		}
		else
		{
			Debug.LogWarning("No Car Materials Found.");
		}
	}

	public void SetColor(int index, bool unlit)
	{
		if (carMaterials == null)
		{
			setupCarMaterial(this);
		}
		if (carMaterials == null)
		{
			return;
		}
		if (ColorBundle.Colors == null || index >= ColorBundle.Colors.Length)
		{
			Debug.LogWarning("Error index :" + index);
		}
		else if (!unlit)
		{
			this.unlit = unlit;
			ResetShader();
			if (index >= 0 && index < ColorBundle.Textures.Length)
			{
				for (int i = 0; i < carMaterials.Length; i++)
				{
					if (carMaterials[i] != null)
					{
						carMaterials[i].mainTexture = ColorBundle.Textures[index];
					}
				}
			}
			else
			{
				Debug.LogWarning("Error index :" + index + "  Length=" + ColorBundle.Textures.Length);
			}
		}
		else if (index >= 0 && index < ColorBundle.Textures.Length)
		{
			for (int j = 0; j < carMaterials.Length; j++)
			{
				if (carMaterials[j] != null)
				{
					carMaterials[j].mainTexture = ColorBundle.Textures[index];
				}
			}
		}
		else
		{
			Debug.LogWarning("Error index :" + index + "  Length=" + ColorBundle.Textures.Length);
		}
	}

	public void SetColor(Color color, bool unlit = false)
	{
		if (carMaterials == null)
		{
			setupCarMaterial(this);
		}
		if (ColorBundle.Textures == null || ColorBundle.Textures.Length == 0)
		{
			return;
		}
		float num = 100f;
		int num2 = -1;
		for (int i = 0; i < ColorBundle.Colors.Length; i++)
		{
			float sqrMagnitude = ((Vector4)color - (Vector4)ColorBundle.Colors[i]).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		Debug.Log("Color index = " + num2);
		if (num2 >= 0 && num2 < ColorBundle.Textures.Length)
		{
			for (int j = 0; j < carMaterials.Length; j++)
			{
				if (carMaterials[j] != null)
				{
					carMaterials[j].mainTexture = ColorBundle.Textures[num2];
				}
			}
		}
		else
		{
			Debug.LogWarning("Error index :" + num2 + "  Length=" + ColorBundle.Textures.Length);
		}
	}

	public void LoadPathReflectColor(Color c)
	{
		if (carMaterials == null)
		{
			setupCarMaterial(this);
		}
		if (carMaterials == null)
		{
			return;
		}
		for (int i = 0; i < carMaterials.Length; i++)
		{
			Material material = carMaterials[i];
			if (material != null && material.HasProperty("_ReflectColor"))
			{
				material.SetColor("_ReflectColor", c);
			}
		}
	}

	private static void setupCarMaterial(CarView v)
	{
	}

	public static void FindCarMaterials(CarView v)
	{
		bool flag = v;
	}

	public static void FindCarRenders(CarView v)
	{
		if ((bool)v)
		{
			v.CarRenders = v.GetComponentsInChildren<Renderer>();
		}
	}

	public void ResetShader()
	{
		if (carMaterials == null)
		{
			setupCarMaterial(this);
		}
		if (Unlit)
		{
			Shader shader = Shader.Find("CarRace/CarPaintSimple");
			if (carMaterials != null)
			{
				for (int i = 0; i < carMaterials.Length; i++)
				{
					carMaterials[i].shader = shader;
				}
			}
			return;
		}
		Shader shader2 = Shader.Find("CarRace/CarPaintShow5");
		if (carMaterials != null)
		{
			for (int j = 0; j < carMaterials.Length; j++)
			{
				carMaterials[j].shader = shader2;
				carMaterials[j].SetFloat("_ReflectColor", RaceConfig.CarReflectScale);
			}
		}
	}

	public void StartFlash()
	{
		OnUpdate = (Action)Delegate.Remove(OnUpdate, new Action(flashUpdate));
		if (CarMaterial != null && CharMaterials != null)
		{
			flashStartTime = Time.time;
			OnUpdate = (Action)Delegate.Combine(OnUpdate, new Action(flashUpdate));
		}
		else
		{
			Debug.LogWarning("No Char or Car Materials Found.");
		}
	}

	public void StopFlash()
	{
		OnUpdate = (Action)Delegate.Remove(OnUpdate, new Action(flashUpdate));
		setAlpha(1f);
	}

	private void flashUpdate()
	{
		float num = Time.time - flashStartTime;
		if (num >= 1.8000001f)
		{
			StopFlash();
			return;
		}
		float num2 = (int)(num / 0.6f);
		float num3 = num - 0.6f * num2;
		float num4 = 0.3f;
		num3 = ((!(num3 <= num4)) ? Mathf.Lerp(0.35f, 1f, (num3 - num4) / num4) : Mathf.Lerp(1f, 0.35f, num3 / num4));
		setAlpha(num3);
	}

	private void setAlpha(float a)
	{
		Color carColor = CarColor;
		carColor.a = a;
		CarColor = carColor;
		if (CharMaterials != null)
		{
			for (int i = 0; i < CharMaterials.Length; i++)
			{
				Material material = CharMaterials[i];
				material.SetFloat("_Cutoff", a);
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (AcOnDrawGizmosSelected != null)
		{
			AcOnDrawGizmosSelected();
		}
	}

	public bool IsEnableDriftAudio()
	{
		if (AudioController == null)
		{
			return false;
		}
		if (!AudioController.Active)
		{
			return false;
		}
		return AudioController.IsEnableDriftAudio();
	}

	public void StopDriftAudio()
	{
		if (AudioController != null)
		{
			AudioController.StopDriftAudio();
		}
	}

	public void EnableDirftAudio()
	{
		if (AudioController != null && AudioController.Active)
		{
			AudioController.SetDriftVolume(on: true);
		}
	}

	public void DisableDirftAudio()
	{
		if (AudioController != null && AudioController.Active)
		{
			AudioController.SetDriftVolume(on: false);
		}
	}

	public void OnUseItem(RaceItemBase item, int itemIndex)
	{
		ItController.OnUseItem(item, itemIndex);
	}

	public object TryGet(RaceItemId itemId, ushort insId)
	{
		if (ItController != null)
		{
			object obj = ItController.TryGetTrigger(itemId, insId);
			if (obj == null)
			{
				obj = ItController.TryGetItem(itemId, insId);
			}
			return obj;
		}
		return null;
	}

	public void SetupSmallGasAirflowEffect(bool loadOnNull = true)
	{
		if (carEffectController != null)
		{
			carEffectController.SetupSmallAirflowEffect(loadOnNull);
		}
	}

	public void SetupHSEffect(bool loadOnNull = true)
	{
		if (carEffectController != null)
		{
			carEffectController.SetupHSEffect(loadOnNull);
		}
	}

	public void SetSkidmarks(Skidmarks[] marks)
	{
		if (SkController != null)
		{
			SkController.SetSkidmarks(marks);
		}
	}

	public void SetVerticalSkidmarks(VerticalSkidmarks[] marks)
	{
		if (SkController != null)
		{
			SkController.SetVerticalSkidmarks(marks);
		}
	}

	public void StopAllItemEffects()
	{
		ItController.StopAllItemEffects();
	}

	public void StopAllTriggers()
	{
		ItController.StopAllTriggers();
	}

	public void StopAllEffects()
	{
		carEffectController.StopAllEffects();
	}

	public void StopAllController()
	{
		Controller.Active = false;
		Reseter.Active = false;
		pathController.Active = false;
		ShakeController.Active = false;
		carEffectController.Active = false;
		SyncController.Active = false;
		Ai.Active = false;
		SkController.Active = false;
		crasherController.Active = false;
		AnimController.Active = false;
		ItController.Active = false;
		N2Scraper.Active = false;
		PathAnimController.Active = false;
		SkillController.Active = false;
	}

	public void InitAnimations()
	{
		AnimController.Init(this, carState, carModel.carModelType);
		AnimController.Active = true;
	}

	public void StopAutoAnim()
	{
		AnimController.Active = false;
	}

	public void PlayPreStart()
	{
		AnimController.PlayPreStart();
	}

	public void PlayWin()
	{
		AnimController.PlayWin();
	}

	public void PlayLose()
	{
		AnimController.PlayLose();
	}

	public void PlayUseItem()
	{
		AnimController.PlayUseItem();
	}

	public void PlayOvertake()
	{
		AnimController.PlayExtra(AnimController.ANIM_OVERTAKE);
	}

	public void PlayOvertaken()
	{
		AnimController.PlayExtra(AnimController.ANIM_OVERTAKEN);
	}

	public void PlayOnDevil()
	{
		AnimController.PlayExtra(AnimController.ANIM_DEVIL);
	}

	public void PlayOnBubble()
	{
		AnimController.PlayExtra(AnimController.ANIM_BUBBLE);
	}

	public void PlayOnUFO()
	{
		AnimController.PlayExtra(AnimController.ANIM_UFO);
	}

	public void PlayUseItemSuccess()
	{
		AnimController.PlayExtra(AnimController.ANIM_USE_ITEM_SUCCESS);
	}

	public void PlayOnCrash()
	{
		AnimController.PlayExtra(AnimController.ANIM_CRASHED);
	}

	public void PlayOnJumping(float duration)
	{
		AnimController.PlayExtra(AnimController.ANIM_AIR_JUMPS[UnityEngine.Random.Range(0, AnimController.ANIM_AIR_JUMPS.Length)]);
	}

	public void PlayTheFirst()
	{
		AnimController.PlayExtra(AnimController.ANIM_THE_FIRST);
	}

	public void PlayTheLast()
	{
		AnimController.PlayExtra(AnimController.ANIM_THE_LAST);
	}

	public void PlayCarShow()
	{
		AnimController.PlayExtra(AnimController.ANIM_CAR_SHOW, WrapMode.Loop);
	}

	public void ToggleItemAsTarget(RaceItemBase item, ItemParams ps, Action<RaceItemBase> onToggle = null)
	{
		ItController.ToggleItemAsTarget(item, ps, onToggle);
	}

	public void OnTogglePassiveItem(GameObject go, float dirY = float.MinValue, long playerId = 0L, Action<TriggerData> onToggle = null, Action<PassiveTrigger> onOver = null)
	{
		ItController.OnTogglePassiveItem(go, dirY, playerId, onToggle, onOver);
	}

	public void AddItem(RaceItemBase item)
	{
		ItController.AddItem(item);
	}

	public bool ApplyingItem(RaceItemId id, List<RaceItemBase> buf = null)
	{
		return ItController.ApplyingItem(id, buf);
	}

	public void AddAiSpeedScale(float delta)
	{
		Ai.AddSpeedScale(delta);
	}

	public void StartAutoRun(Vector3 startPos, float startSpeed = 0f)
	{
		Controller.Active = false;
		Ai.JointMove = true;
		Ai.Active = true;
		Ai.Paused = false;
		Ai.resetJoint();
		Ai.AiMoveNearly(startPos, startSpeed);
	}

	public void StopAutoRun(Vector3 stopPos)
	{
		Ai.Active = false;
		Ai.Paused = true;
		Ai.breakJoint();
		Controller.Active = true;
		carState.rigidbody.drag = 0f;
		carState.rigidbody.mass = carState.view.carModel.CarWeight;
		carState.view.SetSideWayFriction(carState.view.carModel.SideWayFriction);
		RigidbodyTool.RemoveConstraints(carState.view, RigidbodyConstraints.FreezeAll);
		if (stopPos != Vector3.zero)
		{
			base.transform.position = stopPos + base.transform.up * 0.2f;
		}
		else if (CarPlayerType == PlayerType.PLAYER_SELF)
		{
			Reseter.ResetImmediately(Controller);
		}
	}

	public void StartAiMove(Action<CarView> endCallback = null)
	{
		if (CarPlayerType == PlayerType.PALYER_AI)
		{
			Debug.LogWarning("Modify by Sbin : AI collision problem");
			carState.rigidbody.isKinematic = true;
			Ai.StartAiMove(endCallback);
		}
	}

	public void StartBossAiMove(Action<CarView> endCallback = null)
	{
		if (CarPlayerType == PlayerType.PALYER_AI)
		{
			Debug.LogWarning("Modify by Sbin : AI collision problem");
			carState.rigidbody.isKinematic = true;
			Ai.StartBossAiMove(endCallback);
		}
	}

	public void SetupAiTeamGas()
	{
		Ai.SetUpTeamGasScraper();
	}

	public void StartAiRecord()
	{
		Ai.StartAiRecord();
	}

	public void RecordImmediate()
	{
		Ai.RecordImmediate();
	}

	public void StopRecord()
	{
		if (Ai != null)
		{
			Ai.StopRecord();
		}
	}

	public void AddItemPoint(float time, byte index = 0, RaceItemId itemId = RaceItemId.NONE)
	{
		Ai.AddItemPoint(time, index, itemId);
	}

	public void AddQtePoint(float time, byte flag)
	{
		Ai.AddQtePoint(time, flag);
	}

	public float GetLapStartTime(int lap)
	{
		if (Ai != null)
		{
			return Ai.GetLapStartTime(lap);
		}
		return -1f;
	}

	public void StartSmallGas()
	{
		Controller.startSmallN2Force();
	}

	public void SendTransform()
	{
		SyncController.SendTransform();
	}

	public void SyncMovement(byte[] data, bool force)
	{
		SyncController.AsyncMovement(data, force);
	}

	public void SyncMovement(MovementData mvData, bool force)
	{
		SyncController.AsyncMovement(mvData, force);
	}

	public int GetSyncMoveQueueCount()
	{
		return SyncController.GetSyncMoveQueueCount();
	}

	public void ClearSyncMoveQueue()
	{
		SyncController.ClearSyncMoveQueue();
	}

	public void SetDebugNetDelay(long delay)
	{
		if (SyncController != null)
		{
			SyncController.NetDelay = delay;
		}
	}

	public ItemToggle GetAITogglePoint(int index)
	{
		return Ai.GetTogglePoint(index);
	}

	public void ResetPathState()
	{
		pathController.Reset();
	}

	public void ResetDriftParams()
	{
		Controller.Drifter.Init(carState);
	}

	public void ResetPosition()
	{
		Reseter.ResetImmediately(pathController);
	}

	public float GetCommonTopSpeed()
	{
		Controller.Init(this, base.transform, carState, carModel);
		Controller.Init();
		return carModel.MaxSpeedsKmph[0];
	}

	public void PlayLaugh(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayLaugth(isLady);
		}
	}

	public void PlaySpeedUp(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlaySpeedUp(isLady);
		}
	}

	public void PlayOnFly(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayOnFly(isLady);
		}
	}

	public void PlayHurt(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayHurt(isLady);
		}
	}

	public void PlayTired(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayTired(isLady);
		}
	}

	public void PlayAngry(bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayAngry(isLady);
		}
	}

	public void PlayDriftPraise(int lv, bool isLady)
	{
		if (AudioController != null)
		{
			AudioController.PlayPraise(lv, isLady);
		}
	}

	public void StopEngineAudio()
	{
		if (AudioController != null)
		{
			AudioController.StopEngineAudio();
		}
	}

	public void EnableEngineAudio()
	{
		if (AudioController != null)
		{
			AudioController.EnableEngineAudio();
		}
	}

	public void SetWheelState()
	{
		if (Controller != null)
		{
			Controller.setWheelState(carState);
		}
	}

	public void AddSpeed(float added, float max)
	{
		if (CarPlayerType == PlayerType.PLAYER_SELF && !carState.rigidbody.isKinematic)
		{
			float magnitude = carState.rigidbody.velocity.magnitude;
			magnitude = Mathf.Clamp(added, 0f, max - magnitude);
			carState.rigidbody.AddRelativeForce(0f, 0f, magnitude, ForceMode.VelocityChange);
		}
	}

	public string GetHumanTsfName()
	{
		if (ShakeController.Active)
		{
			return "ShakeRoot/Models/Human";
		}
		return "Models/Human";
	}

	public static float CalculateTopSpeed(CarModel param, int level = 0)
	{
		return CarController.CalculateTopSpeed(param, level);
	}

	public void DoRotation(float angle)
	{
		if (carState.CollisionDirection != HitWallDirection.RIGHT)
		{
			if (carState.CollisionDirection != HitWallDirection.LEFT)
			{
				return;
			}
			angle = 0f - angle;
		}
		finalAngle = angle;
		GetComponent<Rigidbody>().velocity = Quaternion.Euler(0f, angle, 0f) * GetComponent<Rigidbody>().velocity;
		carState.velocity = GetComponent<Rigidbody>().velocity;
		if (Application.isEditor)
		{
			Debug.DrawRay(carState.HitPoint, carState.rigidbody.velocity, Color.white, 5f);
		}
	}

	public void RotToAngle(float angle)
	{
		finalAngle = angle;
	}

	private void DoUpdateRot()
	{
		if (!(finalAngle <= 0f))
		{
			float num = finalAngle / 360f;
			if (angleTime > num)
			{
				angleTime = 0f;
				finalAngle = 0f;
				return;
			}
			angleTime += Time.deltaTime;
			Vector3 up = base.transform.up;
			float num2 = Mathf.Lerp(0f, finalAngle, angleTime / num);
			float y = GetComponent<Rigidbody>().rotation.eulerAngles.y;
			finalAngle -= num2;
			Quaternion rot = Quaternion.AngleAxis(y + num2, up);
			GetComponent<Rigidbody>().MoveRotation(rot);
		}
	}

	public Vector3 GetForwardPosition(float targetDis)
	{
		CarState carState = this.carState;
		Vector3 vector = carState.transform.position;
		RacePathNode curNode = carState.CurNode;
		if ((bool)curNode)
		{
			RacePathNode racePathNode = curNode;
			curNode = curNode.LeftNode;
			if (curNode == null)
			{
				return racePathNode.transform.position;
			}
			float num;
			for (num = (curNode.transform.position - vector).magnitude; num < targetDis; num += ((curNode.Distance > racePathNode.Distance) ? (curNode.Distance - racePathNode.Distance) : (curNode.transform.position - racePathNode.transform.position).magnitude))
			{
				racePathNode = curNode;
				curNode = curNode.LeftNode;
				if (curNode == null)
				{
					return racePathNode.transform.position;
				}
			}
			if (num >= targetDis)
			{
				num -= targetDis;
				num = (curNode.transform.position - racePathNode.transform.position).magnitude - num;
				vector = racePathNode.transform.position + num * (racePathNode.LeftNode.transform.position - racePathNode.transform.position).normalized;
				vector = ((!Physics.Raycast(vector + new Vector3(0f, 20f, 0f), Vector3.down, out var hitInfo, 100f, 256)) ? curNode.transform.position : hitInfo.point);
			}
		}
		return vector;
	}

	public void GetForwardPositionRotation(float targetDis, Transform output)
	{
		CarState carState = this.carState;
		output.position = carState.transform.position;
		output.rotation = carState.transform.rotation;
		RacePathNode curNode = carState.CurNode;
		if (!curNode)
		{
			return;
		}
		RacePathNode racePathNode = curNode;
		curNode = curNode.LeftNode;
		if (curNode == null)
		{
			output.position = racePathNode.transform.position;
			return;
		}
		float num;
		for (num = (curNode.transform.position - output.position).magnitude; num < targetDis; num += ((curNode.Distance > racePathNode.Distance) ? (curNode.Distance - racePathNode.Distance) : (curNode.transform.position - racePathNode.transform.position).magnitude))
		{
			racePathNode = curNode;
			curNode = curNode.LeftNode;
			if (curNode == null)
			{
				output.position = racePathNode.transform.position;
				output.rotation = racePathNode.transform.rotation;
				return;
			}
		}
		if (num >= targetDis)
		{
			num -= targetDis;
			num = (curNode.transform.position - racePathNode.transform.position).magnitude - num;
			output.position = racePathNode.transform.position + num * (racePathNode.LeftNode.transform.position - racePathNode.transform.position).normalized;
			output.forward = (curNode.transform.position - racePathNode.transform.position).normalized;
			if (Physics.Raycast(output.position + new Vector3(0f, 20f, 0f), Vector3.down, out var hitInfo, 100f, 256))
			{
				output.position = hitInfo.point;
				return;
			}
			output.position = curNode.transform.position;
			output.rotation = curNode.transform.rotation;
		}
	}

	public void RotateAngleForCarModel(float angle)
	{
		RotateAngleForCarModel(Quaternion.Euler(0f, 0f, angle));
	}

	public void RotateAngleForCarModel(Quaternion quater)
	{
		if (carModel.carModelType == CarModelType.Motorbike)
		{
			models.localRotation = quater;
			if (tranN2Gas == null)
			{
				tranN2Gas = base.transform.Find("N2Gas");
			}
			else
			{
				tranN2Gas.localRotation = quater;
			}
		}
	}

	public float GetCarModelRotateZ()
	{
		return models.localEulerAngles.z;
	}

	public Transform GetHumanTransform()
	{
		return base.transform.Find(GetHumanTsfName());
	}
}
