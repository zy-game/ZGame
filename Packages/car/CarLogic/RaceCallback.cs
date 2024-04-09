using System;
using UnityEngine;

namespace CarLogic
{
	public class RaceCallback
	{
		private static AbstractView view;

		private static Action<CarState> acOnInput;

		private static int _currentTouchCount = 0;

		private static bool _touchCooldown = false;

		private static float _coolDownStartTime = 0f;

		public static float TOUCH_COOLDOWN_TIME = 0.15f;

		private static Func<ControlType> fcControlType;

		private static Action<TriggerData> acSaveTrigger;

		private static Action<AbstractView, TriggerData> acReleaseTrigger;

		private static Action<CarState, ushort, RaceItemId> acSendTriggerToggle;

		private static Func<RaceItemId> fcRandomItem;

		private static Action<byte[]> acSendRemote;

		private static Func<AnimationController, AnimStage, string> fcGetAnimName;

		private static Action<GameObject, object[]> acTweenMove;

		private static Action<GameObject> acTweenStop;

		public static AbstractView View
		{
			get
			{
				if ((bool)view)
				{
					return view;
				}
				view = new GameObject("Mono").AddComponent<AbstractView>();
				return view;
			}
			set
			{
				view = value;
			}
		}

		public static Action<CarState> AcOnInput
		{
			set
			{
				acOnInput = value;
			}
		}

		public static Func<ControlType> FcControlType
		{
			set
			{
				fcControlType = value;
			}
		}

		public static Action<TriggerData> AcSaveTrigger
		{
			set
			{
				acSaveTrigger = value;
			}
		}

		public static Action<AbstractView, TriggerData> AcReleaseTrigger
		{
			set
			{
				acReleaseTrigger = value;
			}
		}

		public static Action<CarState, ushort, RaceItemId> AcSendTriggerToggle
		{
			set
			{
				acSendTriggerToggle = value;
			}
		}

		public static Func<RaceItemId> FcRandomItem
		{
			set
			{
				fcRandomItem = value;
			}
		}

		public static Action<byte[]> AcSendRemote
		{
			set
			{
				acSendRemote = value;
			}
		}

		public static Func<AnimationController, AnimStage, string> FcGetAnimName
		{
			set
			{
				fcGetAnimName = value;
			}
		}

		public static Action<GameObject, object[]> AcTweenMove
		{
			set
			{
				acTweenMove = value;
			}
		}

		public static Action<GameObject> AcTweenStop
		{
			set
			{
				acTweenStop = value;
			}
		}

		internal static void GetInput(CarState state)
		{
			if (_currentTouchCount < Input.touchCount)
			{
				bool flag = false;
				flag |= state.AirGround.TryStartGas();
				if (_touchCooldown = flag | state.view.Controller.TryStartSecondSmallGas())
				{
					_coolDownStartTime = Time.time;
					return;
				}
			}
			if (_touchCooldown && _currentTouchCount > Input.touchCount)
			{
				_touchCooldown = false;
			}
			_currentTouchCount = Input.touchCount;
			if (!(Time.time - _coolDownStartTime < TOUCH_COOLDOWN_TIME) && !_touchCooldown && acOnInput != null)
			{
				acOnInput(state);
			}
		}

		internal static ControlType GetControlType()
		{
			if (fcControlType != null)
			{
				return fcControlType();
			}
			return ControlType.TouchType;
		}

		internal static void SaveTriggerToBuf(TriggerData trigger)
		{
			if (acSaveTrigger != null)
			{
				acSaveTrigger(trigger);
			}
		}

		internal static void ReleaseTrigger(AbstractView view, TriggerData data)
		{
			if (acReleaseTrigger != null)
			{
				acReleaseTrigger(view, data);
			}
		}

		internal static void SendTriggerToggle(CarState carState, ushort instanceId, RaceItemId itemId)
		{
			if (acSendTriggerToggle != null)
			{
				acSendTriggerToggle(carState, instanceId, itemId);
			}
		}

		internal static RaceItemId RandomItemByRank()
		{
			if (fcRandomItem != null)
			{
				return fcRandomItem();
			}
			return RaceItemId.NONE;
		}

		internal static void SendRemote(byte[] data)
		{
			if (acSendRemote != null)
			{
				acSendRemote(data);
			}
		}

		internal static string GetAnimName(AnimationController controller, AnimStage ev)
		{
			if (fcGetAnimName != null)
			{
				return fcGetAnimName(controller, ev);
			}
			return "";
		}

		internal static void TweenMove(GameObject go, object[] objs)
		{
			if (acTweenMove != null)
			{
				acTweenMove(go, objs);
			}
		}

		internal static void TweenStop(GameObject go)
		{
			if (acTweenStop != null)
			{
				acTweenStop(go);
			}
		}
	}
}
