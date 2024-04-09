using System;
using System.Collections.Generic;
using CarLogic;
using UnityEngine;

[AddComponentMenu("CarLogic/Path Manager")]
public class RacePathManager : MonoBehaviour
{
	public static RacePathManager ActiveInstance = null;

	public static float ResetLowLimit = -20f;

	public int MapId;

	public int AICount;

	public float CameraFarClip = 300f;

	public float LowLimit = -10f;

	public bool HasGlowEffect;

	public Material SkyboxMaterial;

	public string NamePreFix = "Node";

	public GameObject PathNodePrefab;

	public RacePathNode EndNode;

	public RacePathNode StartNode;

	public Transform ItemBoxRoot;

	public GameObject ItemBoxPrefab;

	public Material ItemBoxMaterial;

	public Color ReflectColor = new Color(1f, 1f, 1f, 0.5f);

	public ScriptableObject ShadowMeshTree;

	[HideInInspector]
	public MeshRenderer TargetMesh;

	[HideInInspector]
	public List<RacePathNode> PointList = new List<RacePathNode>();

	[HideInInspector]
	public int maskFlag;

	public float TotalLength;

	public Texture2D[] LightMaps;

	[HideInInspector]
	public PathDependence[] Dependences;

	public PathExtra UserData;

	public Dictionary<int, TranslateStarter> TranslateMap = new Dictionary<int, TranslateStarter>();

	public AnimationClip[] StartAnim;

	public Action OnRelease;

	public AIBundle AIs
	{
		get
		{
			if (Dependences != null && Dependences.Length > 3 && Dependences[3] != null)
			{
				return Dependences[3].ResObject as AIBundle;
			}
			return null;
		}
	}

	public void Awake()
	{
		ResetLowLimit = base.transform.position.y + LowLimit;
		if (StartNode == null && PointList.Count > 0)
		{
			StartNode = PointList[0];
		}
		if (EndNode == null && PointList.Count > 0)
		{
			EndNode = PointList[0];
		}
		if (Application.isPlaying)
		{
			ActiveInstance = this;
		}
	}

	public void Start()
	{
		if ((bool)Camera.main)
		{
			Camera.main.farClipPlane = CameraFarClip * RaceConfig.FarClipScaler;
		}
		if (Dependences != null && Dependences.Length > 4 && Dependences[4] != null)
		{
			Transform transform = Dependences[4].ResObject as Transform;
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
		}
		loadWeather();
	}

	public void ShowWeather()
	{
		if (UserData != null && UserData.WeatherObject != null)
		{
			UserData.WeatherObject.SetActive(value: true);
		}
	}

	public void PauseWeather()
	{
		if (UserData != null && UserData.WeatherObject != null)
		{
			UserData.WeatherObject.SetActive(value: false);
		}
	}

	private void loadWeather()
	{
		if (!RaceConfig.WeatherOn || Dependences == null || Dependences.Length <= 1)
		{
			return;
		}
		PathDependence pathDependence = Dependences[1];
		if (pathDependence != null)
		{
			LoadDependence(this, 1, delegate(UnityEngine.Object o)
			{
				if (!(o == null))
				{
					PathExtra pathExtra = UserData;
					if (pathExtra == null)
					{
						pathExtra = (UserData = new PathExtra());
					}
					if (pathExtra.WeatherObject != null)
					{
						UnityEngine.Object.Destroy(pathExtra.WeatherObject);
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					Camera camera = findUICamera(base.transform);
					Transform transform = gameObject.transform;
					transform.parent = camera.transform;
					transform.position = camera.transform.position + camera.transform.forward;
					pathExtra.WeatherObject = camera.gameObject;
				}
			}, "");
		}
		else
		{
			Debug.LogWarning("Auto Load is " + pathDependence);
		}
	}

	private static Camera findUICamera(Transform p)
	{
		Camera camera = null;
		Camera[] allCameras = Camera.allCameras;
		int num = LayerMask.NameToLayer("UI");
		for (int i = 0; i < allCameras.Length; i++)
		{
			if (allCameras[i].gameObject.layer == num && allCameras[i].gameObject.name.Contains("RaceCamera"))
			{
				camera = allCameras[i];
				break;
			}
		}
		if (camera == null)
		{
			GameObject gameObject = new GameObject("PathCamera");
			gameObject.transform.parent = p;
			gameObject.transform.localPosition = new Vector3(3000f, -3000f, 100f);
			camera = gameObject.AddComponent<Camera>();
			camera.cullingMask = 1 << num;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.depth = 3f;
			camera.orthographicSize = 1f;
			camera.nearClipPlane = -2f;
			camera.farClipPlane = 2f;
			camera.orthographic = true;
		}
		return camera;
	}

	public RacePathNode GetPathNode(string nodeName)
	{
		return PointList.Find((RacePathNode node) => node.name == nodeName);
	}

	public void OnDestroy()
	{
		if (OnRelease != null)
		{
			OnRelease();
		}
		OnRelease = null;
		if (Application.isPlaying && (object)this == ActiveInstance)
		{
			ActiveInstance = null;
		}
	}

	public static void LoadDependence(RacePathManager path, int index, ResCallback cb, string resPrefix, string def = "")
	{
		UnityEngine.Object @object = null;
		string text = def;
		if (path != null)
		{
			PathDependence pathDependence = null;
			if (path.Dependences != null && path.Dependences.Length > index)
			{
				pathDependence = path.Dependences[index];
				if (pathDependence.ResObject != null)
				{
					@object = pathDependence.ResObject;
				}
				else if (!string.IsNullOrEmpty(pathDependence.ResPath))
				{
					text = resPrefix + pathDependence.ResPath;
				}
			}
		}
		if (@object != null)
		{
			cb(@object);
		}
		else if (!string.IsNullOrEmpty(text))
		{
			Singleton<ResourceOffer>.Instance.Load(text, cb);
		}
		else
		{
			Debug.LogWarning("RacePathManager: Empty dependence resources path.");
		}
	}
}
