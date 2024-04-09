using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[AddComponentMenu("CarLogic/Camera Switcher")]
	public class CameraSwitcher : MonoBehaviour
	{
		public static float ShowDepth;

		public static float HideDepth;

		private static CameraSwitcher instance;

		private static Dictionary<KeyCode, int> KeyMap;

		private static List<KeyCode> KeyCodes;

		public CameraTarget[] Cameras;

		private int curCamera = -1;

		public static CameraSwitcher ActiveInstance => instance;

		static CameraSwitcher()
		{
			ShowDepth = 5f;
			HideDepth = 0f;
			instance = null;
			KeyMap = new Dictionary<KeyCode, int>();
			KeyCodes = new List<KeyCode>(20);
			KeyMap.Add(KeyCode.Alpha1, 1);
			KeyMap.Add(KeyCode.Alpha2, 2);
			KeyMap.Add(KeyCode.Alpha3, 3);
			KeyMap.Add(KeyCode.Alpha4, 4);
			KeyMap.Add(KeyCode.Alpha5, 5);
			KeyMap.Add(KeyCode.Alpha6, 6);
			KeyMap.Add(KeyCode.Alpha7, 7);
			KeyMap.Add(KeyCode.Alpha8, 8);
			KeyMap.Add(KeyCode.Alpha9, 9);
			KeyMap.Add(KeyCode.Alpha0, 10);
			KeyMap.Add(KeyCode.Keypad1, 1);
			KeyMap.Add(KeyCode.Keypad2, 2);
			KeyMap.Add(KeyCode.Keypad3, 3);
			KeyMap.Add(KeyCode.Keypad4, 4);
			KeyMap.Add(KeyCode.Keypad5, 5);
			KeyMap.Add(KeyCode.Keypad6, 6);
			KeyMap.Add(KeyCode.Keypad7, 7);
			KeyMap.Add(KeyCode.Keypad8, 8);
			KeyMap.Add(KeyCode.Keypad9, 9);
			KeyMap.Add(KeyCode.Keypad0, 0);
			KeyCodes.Add(KeyCode.Alpha1);
			KeyCodes.Add(KeyCode.Alpha2);
			KeyCodes.Add(KeyCode.Alpha3);
			KeyCodes.Add(KeyCode.Alpha4);
			KeyCodes.Add(KeyCode.Alpha5);
			KeyCodes.Add(KeyCode.Alpha6);
			KeyCodes.Add(KeyCode.Alpha7);
			KeyCodes.Add(KeyCode.Alpha8);
			KeyCodes.Add(KeyCode.Alpha9);
			KeyCodes.Add(KeyCode.Alpha0);
			KeyCodes.Add(KeyCode.Keypad1);
			KeyCodes.Add(KeyCode.Keypad2);
			KeyCodes.Add(KeyCode.Keypad3);
			KeyCodes.Add(KeyCode.Keypad4);
			KeyCodes.Add(KeyCode.Keypad5);
			KeyCodes.Add(KeyCode.Keypad6);
			KeyCodes.Add(KeyCode.Keypad7);
			KeyCodes.Add(KeyCode.Keypad8);
			KeyCodes.Add(KeyCode.Keypad9);
			KeyCodes.Add(KeyCode.Keypad0);
		}

		private static int GetIndexOfKey()
		{
			int value = -1;
			for (int i = 0; i < KeyCodes.Count; i++)
			{
				KeyCode key = KeyCodes[i];
				if (Input.GetKeyDown(key))
				{
					if (KeyMap.TryGetValue(key, out value))
					{
						break;
					}
					value = -1;
				}
			}
			return value;
		}

		public void Awake()
		{
			instance = this;
		}

		public void OnDestroy()
		{
			if (instance == this)
			{
				instance = null;
			}
		}

		public void Start()
		{
			if (Camera.main != null)
			{
				HideDepth = Camera.main.depth;
			}
			if (Cameras == null)
			{
				return;
			}
			for (int i = 0; i < Cameras.Length; i++)
			{
				if (Cameras[i] == null || !(Cameras[i].camera != null))
				{
					continue;
				}
				Cameras[i].camera.enabled = false;
				Cameras[i].camera.cullingMask = -1 & ~((1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("MiniMap")) | (1 << LayerMask.NameToLayer("TransparentFX")));
				Cameras[i].camera.clearFlags = CameraClearFlags.Skybox;
				if (RacePathManager.ActiveInstance != null)
				{
					Skybox skybox = Cameras[i].camera.gameObject.AddComponent<Skybox>();
					skybox.material = RacePathManager.ActiveInstance.SkyboxMaterial;
				}
				if (Cameras[i].Areas == null)
				{
					continue;
				}
				int index = i;
				for (int j = 0; j < Cameras[i].Areas.Length; j++)
				{
					if (!(Cameras[i].Areas[j] != null))
					{
						continue;
					}
					TriggerDelegate triggerDelegate = Cameras[i].Areas[j].gameObject.AddComponent<TriggerDelegate>();
					triggerDelegate.AcOnTriggerEnter = delegate(Collider c)
					{
						if (c.gameObject.layer == 14)
						{
							CarView component2 = c.transform.root.GetComponent<CarView>();
							if (component2 != null && component2.CarPlayerType == PlayerType.PLAYER_SELF)
							{
								ShowCamera(index);
							}
						}
					};
					triggerDelegate.AcOnTriggerExit = delegate(Collider c)
					{
						if (c.gameObject.layer == 14)
						{
							CarView component = c.transform.root.GetComponent<CarView>();
							if (component != null && component.CarPlayerType == PlayerType.PLAYER_SELF && index == curCamera)
							{
								BackToMainCamera();
							}
						}
					};
				}
			}
		}

		public void Update()
		{
			if (!Input.anyKeyDown)
			{
				return;
			}
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				BackToMainCamera();
				return;
			}
			int indexOfKey = GetIndexOfKey();
			if (indexOfKey > 0 && indexOfKey <= Cameras.Length)
			{
				ShowCamera(indexOfKey - 1);
			}
		}

		public void BackToMainCamera()
		{
			hideCurrent();
			if (Camera.main != null)
			{
				Camera.main.depth = HideDepth;
				Camera.main.enabled = true;
			}
		}

		public void NextCamera()
		{
			if (Cameras != null && Cameras.Length != 0)
			{
				hideCurrent();
				curCamera++;
				showCurrent();
			}
		}

		public void PreCamera()
		{
			if (Cameras != null && Cameras.Length != 0)
			{
				hideCurrent();
				curCamera--;
				showCurrent();
			}
		}

		public void ShowCamera(int index)
		{
			if (Cameras != null && Cameras.Length != 0)
			{
				hideCurrent();
				Debug.Log("Show Index of " + index);
				curCamera = index;
				showCurrent();
			}
		}

		private void showCurrent()
		{
			curCamera %= Cameras.Length;
			if (curCamera < 0)
			{
				curCamera += Cameras.Length;
			}
			CameraTarget cameraTarget = Cameras[curCamera % Cameras.Length];
			if (cameraTarget != null && cameraTarget.camera != null)
			{
				cameraTarget.camera.depth = ShowDepth;
				cameraTarget.camera.enabled = true;
			}
		}

		private void hideCurrent()
		{
			if (curCamera >= 0)
			{
				CameraTarget cameraTarget = Cameras[curCamera % Cameras.Length];
				if (cameraTarget != null && cameraTarget.camera != null)
				{
					cameraTarget.camera.depth = HideDepth;
					cameraTarget.camera.enabled = false;
				}
			}
		}
	}
}
