using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[AddComponentMenu("CarLogic/Free Camera")]
	public class FreeCamera : MonoBehaviour
	{
		private List<CarView> cars = new List<CarView>(6);

		private CameraController cc;

		public float DistanceDamp = 2f;

		public float HeightDamp = 0.8f;

		public float RotDamp = 90f;

		private float orgDis;

		private float orgHt;

		private bool useScroll;

		public void Start()
		{
			if ((bool)CameraSwitcher.ActiveInstance)
			{
				Object.Destroy(CameraSwitcher.ActiveInstance);
			}
		}

		public void SetCamera(CameraController cc)
		{
			this.cc = cc;
			orgDis = cc.distance;
			orgHt = cc.height;
		}

		public void Update()
		{
			if (!(cc == null) && cc.ViewTarget != null)
			{
				if (Input.GetKeyUp(KeyCode.Alpha1))
				{
					setTarget(0);
				}
				else if (Input.GetKeyUp(KeyCode.Alpha2))
				{
					setTarget(1);
				}
				else if (Input.GetKeyUp(KeyCode.Alpha3))
				{
					setTarget(2);
				}
				else if (Input.GetKeyUp(KeyCode.Alpha4))
				{
					setTarget(3);
				}
				else if (Input.GetKeyUp(KeyCode.Alpha5))
				{
					setTarget(4);
				}
				else if (Input.GetKeyUp(KeyCode.Alpha6))
				{
					setTarget(5);
				}
				float num = (useScroll ? Input.GetAxis("Mouse ScrollWheel") : 0f);
				if (num != 0f)
				{
					addDistance((0f - num) * Time.deltaTime * DistanceDamp * 20f);
				}
				else if (Input.GetKey(KeyCode.Minus))
				{
					addDistance((0f - Time.deltaTime) * DistanceDamp);
				}
				else if (Input.GetKey(KeyCode.Equals))
				{
					addDistance(Time.deltaTime * DistanceDamp);
				}
				if (Input.GetMouseButtonUp(1))
				{
					useScroll = !useScroll;
				}
				if (Input.GetKey(KeyCode.PageUp))
				{
					addHeight(Time.deltaTime * HeightDamp);
				}
				else if (Input.GetKey(KeyCode.PageDown))
				{
					addHeight((0f - Time.deltaTime) * HeightDamp);
				}
				if (Input.GetKey(KeyCode.Q))
				{
					addFreeRot(Time.deltaTime * RotDamp);
				}
				else if (Input.GetKey(KeyCode.E))
				{
					addFreeRot((0f - Time.deltaTime) * RotDamp);
				}
				if (Input.GetKeyDown(KeyCode.F))
				{
					applyN2Gas();
				}
				else if (Input.GetKeyDown(KeyCode.G))
				{
					activaSkidmark();
				}
				else if (Input.GetKeyDown(KeyCode.H))
				{
					HideShowCar();
				}
				if (Input.GetKeyUp(KeyCode.BackQuote))
				{
					resetRot();
				}
				else if (Input.GetMouseButtonDown(2))
				{
					resetDH();
				}
				if (Input.GetKeyUp(KeyCode.Tab))
				{
					switchRotDamp();
				}
			}
		}

		public void OnGUI()
		{
			if (Input.GetKey(KeyCode.F1))
			{
				GUI.skin.box.fontSize = 20;
				float num = Screen.width;
				float num2 = Screen.height;
				GUI.Box(new Rect(num * 0.25f, num2 * 0.25f, num * 0.65f, num2 * 0.45f), "\r\n摄像头操控：\r\n目标切换：1~6，1是自己\r\n距离远近：鼠标滚轮(右键切换是否启用)/ 加减号\r\n高度：PageUp/PageDown\r\n鼠标中键重置距离和高度值\r\n 镜头左右旋转：Q/E, ~键重置，\r\nTab键切换是否锁定转动\r\nF键触发大喷，G键开启漂移特效，H键隐藏显示赛车人物\r\nF2隐藏UI，F3隐藏显示名字\r\n                ");
			}
		}

		private void findTargets()
		{
			cars.Clear();
			CarView[] array = Object.FindObjectsOfType<CarView>();
			if (array == null)
			{
				return;
			}
			CarView[] array2 = array;
			foreach (CarView carView in array2)
			{
				if (carView != null)
				{
					if (carView.CarPlayerType == PlayerType.PLAYER_SELF)
					{
						cars.Insert(0, carView);
					}
					else
					{
						cars.Add(carView);
					}
				}
			}
		}

		private void setTarget(int index)
		{
			if ((bool)CameraSwitcher.ActiveInstance)
			{
				Object.Destroy(CameraSwitcher.ActiveInstance);
			}
			if (cc == null)
			{
				cc = CameraController.Current;
			}
			if (cars.Count == 0)
			{
				findTargets();
			}
			if (cc != null && index >= 0 && index < cars.Count)
			{
				CarView carView = cars[index % cars.Count];
				if (carView != null)
				{
					cc.Init(carView, carView.carState);
					cc.ResetCurRotDamp();
					cc.UseVelocityDir = carView.ItController.ApplyingTrigger(RaceItemId.BANANA);
					cc.ExRotation = Quaternion.identity;
				}
			}
		}

		private void activaSkidmark()
		{
			if ((bool)cc && cc.ViewTarget != null && cc.ViewTarget.transform != null)
			{
				cc.ViewTarget.view.SkController.Active = true;
			}
		}

		private void HideShowCar()
		{
			if ((bool)cc && cc.ViewTarget != null && cc.ViewTarget.transform != null)
			{
				string[] array = new string[6] { "ShakeRoot/Models", "br Column", "bl Column", "fl Column", "fr Column", "CarShadow" };
				for (int i = 0; i < array.Length; i++)
				{
					Transform transform = cc.ViewTarget.view.transform.Find(array[i]);
					transform.gameObject.SetActive(!transform.gameObject.activeSelf);
				}
			}
		}

		private void applyN2Gas()
		{
			if ((bool)cc && cc.ViewTarget != null && (bool)cc.ViewTarget.transform && cc.ViewTarget.CarPlayerType == PlayerType.PALYER_AI)
			{
				CarState viewTarget = cc.ViewTarget;
				CommonGasItem commonGasItem = RaceItemFactory.BuildItemById(RaceItemId.GAS) as CommonGasItem;
				ItemParams itemParams = new ItemParams(null, null, 0);
				itemParams.user = viewTarget;
				itemParams.targets = new CarState[1] { viewTarget };
				commonGasItem = new CommonGasItem(1, 5f, useShake: false);
				if (commonGasItem.Usable(itemParams))
				{
					commonGasItem.duration = viewTarget.view.carModel.SmallN2ForceTime;
					commonGasItem.Toggle(itemParams);
				}
			}
		}

		private void addDistance(float ex)
		{
			if ((bool)cc)
			{
				cc.height = cc.height * (cc.distance + ex) / Mathf.Max(cc.distance, 0.1f);
				cc.distance = Mathf.Max(0.1f, ex + cc.distance);
			}
		}

		private void addHeight(float ex)
		{
			if ((bool)cc)
			{
				cc.height = Mathf.Max(0f, ex + cc.height);
			}
		}

		private void addFreeRot(float ex)
		{
			if (cc != null)
			{
				cc.Freeview = true;
				cc.ExRotation *= Quaternion.AngleAxis(ex, Vector3.up);
			}
		}

		private void switchRotDamp()
		{
			if ((bool)cc)
			{
				cc.dampRotation = !cc.dampRotation;
			}
		}

		private void resetDH()
		{
			if ((bool)cc)
			{
				cc.distance = orgDis;
				cc.height = orgHt;
			}
		}

		private void resetRot()
		{
			if ((bool)cc)
			{
				cc.ExRotation = Quaternion.identity;
			}
		}
	}
}
