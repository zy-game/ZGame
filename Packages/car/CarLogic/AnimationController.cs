using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class AnimationController : ControllerBase
	{
		public string[] ANIM_RACE_READY = new string[4] { "Ready01", "Ready02", "Ready03", "Ready04" };

		public string ANIM_USE_ITEM = "";

		public string ANIM_USE_ITEM_BACK = "";

		public string ANIM_DRIVE = "";

		public string ANIM_BACKOVER = "";

		public string ANIM_BACKSTART = "";

		public string ANIM_BACKHOLDING = "";

		public string ANIM_LEFTOVER = "";

		public string ANIM_LEFTSTART = "";

		public string ANIM_LEFTHOLDING = "";

		public string ANIM_RIGHTOVER = "";

		public string ANIM_RIGHTSTART = "";

		public string ANIM_RIGHTHOLDING = "";

		public string ANIM_LEFTDRIFTOVER = "";

		public string ANIM_LEFTDRIFTSTART = "";

		public string ANIM_LEFTDRIFTHOLDING = "";

		public string ANIM_RIGHTDRIFTOVER = "";

		public string ANIM_RIGHTDRIFTSTART = "";

		public string ANIM_RIGHTDRIFTHOLDING = "";

		public string ANIM_SPRINTSOVER = "";

		public string ANIM_SPRINTSSTART = "";

		public string ANIM_SPRINTSHOLDING = "";

		public string ANIM_NEOSPEEDUP = "";

		public string ANIM_NEOSPEEDUPHOLD = "";

		public string ANIM_NEOSPEEDUPRESET = "";

		public string ANIM_LOSE = "";

		public string ANIM_WIN0 = "";

		public string ANIM_HALF_WIN = "VictorySit";

		public string ANIM_OVERTAKE = "OverTake01";

		public string ANIM_OVERTAKEN = "BeOverTake01";

		public string ANIM_DEVIL = "Devil";

		public string ANIM_BUBBLE = "Bubble";

		public string ANIM_UFO = "UFO";

		public string ANIM_USE_ITEM_SUCCESS = "UseItemSuccess";

		public string ANIM_CRASHED = "Crash";

		public string ANIM_AIR_JUMP = "AirJump01_1";

		public string[] ANIM_AIR_JUMPS = new string[1] { "AirJump01_1" };

		public string ANIM_THE_FIRST = "TheFirst";

		public string ANIM_THE_LAST = "TheLast";

		public string ANIM_CAR_SHOW = "personsit";

		public CarModelType carModelType;

		[NonSerialized]
		private AbstractView view;

		private CarState carState;

		[NonSerialized]
		private Animation humanAnimation;

		private float lastSteer;

		private float lastThrottle;

		private AnimStage lastStage;

		private int itemIID;

		private RaceItemId itemId = RaceItemId.END;

		private int extraFlag;

		private Dictionary<AnimStage, string> animNameMap = new Dictionary<AnimStage, string>();

		private static HashSet<string> animNames = new HashSet<string>();

		private string playingName = "";

		[NonSerialized]
		private Transform _characterObj;

		internal Animation HumanAnimation => humanAnimation;

		public Transform CharacterObj => _characterObj;

		public void Init(AbstractView view, CarState state, CarModelType carModelType = CarModelType.Car)
		{
			this.view = view;
			carState = state;
			this.carModelType = carModelType;
			_characterObj = state.transform.Find(state.view.GetHumanTsfName() + "/char");
			if (_characterObj != null)
			{
				_characterObj = _characterObj.GetChild(0);
				humanAnimation = _characterObj.GetComponent<Animation>();
				AnimationsManager.GetInstance().ReInitAnimationNames(this, _characterObj, carModelType);
			}
			if (humanAnimation != null && animNames.Count == 0)
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				stringBuilder.Append(humanAnimation.name).AppendLine(" >> All Anim: ");
				foreach (AnimationState item in humanAnimation)
				{
					animNames.Add(item.name);
					stringBuilder.AppendLine(item.name);
				}
			}
			checkClip();
		}

		public override void OnActiveChange(bool active)
		{
			AbstractView abstractView = view;
			abstractView.OnUpdate = (Action)Delegate.Remove(abstractView.OnUpdate, new Action(onUpdate));
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnUseItem = (Action<RaceItemBase>)Delegate.Remove(callBacks.OnUseItem, new Action<RaceItemBase>(onUseItem));
			if (active)
			{
				AbstractView abstractView2 = view;
				abstractView2.OnUpdate = (Action)Delegate.Combine(abstractView2.OnUpdate, new Action(onUpdate));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnUseItem = (Action<RaceItemBase>)Delegate.Combine(callBacks2.OnUseItem, new Action<RaceItemBase>(onUseItem));
				checkClip();
			}
		}

		private void onUseItem(RaceItemBase item)
		{
			if (view == null || !base.Active)
			{
				return;
			}
			if (item.ItemId == RaceItemId.ROCKET || item.ItemId == RaceItemId.WATER_FLY || item.ItemId == RaceItemId.WATER_BOMB || item.ItemId == RaceItemId.UFO || item.ItemId == RaceItemId.INK_BOTTLE || item.ItemId == RaceItemId.CONTROL_REVERT || item.ItemId == RaceItemId.ANGEL || item.ItemId == RaceItemId.UFO_CONFUSER || item.ItemId == RaceItemId.GROUP_ANGLE || item.ItemId == RaceItemId.STORM)
			{
				int iid2 = (itemIID = item.IID);
				itemId = item.ItemId;
				carState.AnimationFlag = AnimStage.UseItem;
				carState.view.CallDelay(delegate
				{
					if (!(view == null) && base.Active && iid2 == itemIID && carState.AnimationFlag == AnimStage.UseItem)
					{
						carState.AnimationFlag = AnimStage.CommonDrive;
					}
				}, humanAnimation[ANIM_USE_ITEM].length + Time.deltaTime * 2f);
			}
			else
			{
				if (item.ItemId != RaceItemId.BANANA && item.ItemId != RaceItemId.MINE && item.ItemId != RaceItemId.FOG && item.ItemId != RaceItemId.CHEAT_BOX && item.ItemId != RaceItemId.BLOCK)
				{
					return;
				}
				int iid = (itemIID = item.IID);
				itemId = item.ItemId;
				carState.AnimationFlag = AnimStage.UseItem;
				carState.view.CallDelay(delegate
				{
					if (!(view == null) && base.Active && iid == itemIID && carState.AnimationFlag == AnimStage.UseItem)
					{
						carState.AnimationFlag = AnimStage.CommonDrive;
					}
				}, humanAnimation[ANIM_USE_ITEM_BACK].length + Time.deltaTime * 2f);
			}
		}

		private void onUpdate()
		{
			if (!base.Active)
			{
				return;
			}
			if (null == humanAnimation)
			{
				base.Active = false;
				return;
			}
			if (carState.AnimationFlag == AnimStage.UseItem)
			{
				if (lastStage != AnimStage.UseItem)
				{
					PlayUseItem();
				}
			}
			else if (carState.AnimationFlag != AnimStage.HighPrior)
			{
				if (carState.Throttle < 0f)
				{
					if (lastThrottle >= 0f)
					{
						playDriveBack();
					}
				}
				else if (carState.AnimationFlag == AnimStage.BigHighSpeed)
				{
					if (lastStage != AnimStage.BigHighSpeed)
					{
						PlayBigHSpeed();
					}
				}
				else if (lastStage == AnimStage.BigHighSpeed && carState.AnimationFlag != AnimStage.BigHighSpeed)
				{
					PlayBigHSpeed(revert: true);
				}
				else if (carState.AnimationFlag == AnimStage.HighSpeed)
				{
					if (lastStage != AnimStage.HighSpeed)
					{
						PlayHSpeed();
					}
				}
				else if (lastStage == AnimStage.HighSpeed && carState.AnimationFlag != AnimStage.HighSpeed)
				{
					PlayHSpeed(revert: true);
				}
				else if (lastThrottle < 0f && carState.Throttle >= 0f)
				{
					playDriveBack(revert: true);
				}
				else if (carState.Steer < 0f)
				{
					if (lastSteer >= 0f)
					{
						if (carState.CurDriftState.Stage != 0)
						{
							playDrift(carState.Steer);
						}
						else
						{
							playSteer(carState.Steer);
						}
					}
				}
				else if (carState.Steer > 0f)
				{
					if (lastSteer <= 0f)
					{
						if (carState.CurDriftState.Stage != 0)
						{
							playDrift(carState.Steer);
						}
						else
						{
							playSteer(carState.Steer);
						}
					}
				}
				else if (carState.Steer == 0f)
				{
					if (lastSteer != 0f)
					{
						if (carState.CurDriftState.Stage != 0)
						{
							playDrift(lastSteer, revert: true);
						}
						else
						{
							playSteer(lastSteer, revert: true);
						}
					}
				}
				else if (carState.Throttle >= 0f)
				{
					if (lastThrottle < 0f)
					{
						playDriveBack(revert: true);
					}
				}
				else if (!humanAnimation.IsPlaying(ANIM_DRIVE))
				{
					PlayNormalDrive();
				}
			}
			lastSteer = carState.Steer;
			lastThrottle = carState.Throttle;
			lastStage = carState.AnimationFlag;
		}

		private void PlayNormalDrive()
		{
			if (humanAnimation != null)
			{
				humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
				humanAnimation.CrossFade(ANIM_DRIVE);
			}
		}

		private void playDriveBack(bool revert = false)
		{
			if (humanAnimation != null)
			{
				if (revert)
				{
					humanAnimation[ANIM_BACKOVER].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_BACKOVER);
					humanAnimation.PlayQueued(ANIM_DRIVE);
				}
				else
				{
					humanAnimation[ANIM_BACKSTART].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_BACKHOLDING].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_BACKSTART);
					humanAnimation.PlayQueued(ANIM_BACKHOLDING);
				}
			}
		}

		private void playSteer(float steer, bool revert = false)
		{
			if (!(humanAnimation != null))
			{
				return;
			}
			string text = ((steer < 0f) ? ANIM_LEFTSTART : ANIM_RIGHTSTART);
			string text2 = ((steer < 0f) ? ANIM_LEFTHOLDING : ANIM_RIGHTHOLDING);
			if (revert)
			{
				if (humanAnimation.IsPlaying(text2))
				{
					string text3 = ((steer < 0f) ? ANIM_LEFTOVER : ANIM_RIGHTOVER);
					humanAnimation[text3].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
					humanAnimation.Play(text3);
					humanAnimation.PlayQueued(ANIM_DRIVE);
				}
				else
				{
					humanAnimation.Play(ANIM_DRIVE);
				}
			}
			else
			{
				humanAnimation[text].wrapMode = WrapMode.Once;
				humanAnimation[text2].wrapMode = WrapMode.Loop;
				humanAnimation.Play(text);
				humanAnimation.PlayQueued(text2);
			}
		}

		private void playDrift(float steer, bool revert = false)
		{
			if (!(humanAnimation != null))
			{
				return;
			}
			string text = ((steer < 0f) ? ANIM_LEFTDRIFTSTART : ANIM_RIGHTDRIFTSTART);
			string text2 = ((steer < 0f) ? ANIM_LEFTDRIFTHOLDING : ANIM_RIGHTDRIFTHOLDING);
			if (revert)
			{
				if (humanAnimation.IsPlaying(text2))
				{
					string text3 = ((steer < 0f) ? ANIM_LEFTDRIFTOVER : ANIM_RIGHTDRIFTOVER);
					humanAnimation[text3].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
					humanAnimation.Play(text3);
					humanAnimation.PlayQueued(ANIM_DRIVE);
				}
				else
				{
					humanAnimation.Play(ANIM_DRIVE);
				}
			}
			else
			{
				humanAnimation[text].wrapMode = WrapMode.Once;
				humanAnimation[text2].wrapMode = WrapMode.Loop;
				humanAnimation.Play(text);
				humanAnimation.PlayQueued(text2);
			}
		}

		internal void PlayUseItem()
		{
			if (humanAnimation != null)
			{
				if (itemId == RaceItemId.ROCKET || itemId == RaceItemId.WATER_FLY || itemId == RaceItemId.WATER_BOMB || itemId == RaceItemId.UFO || itemId == RaceItemId.INK_BOTTLE || itemId == RaceItemId.CONTROL_REVERT || itemId == RaceItemId.ANGEL || itemId == RaceItemId.UFO_CONFUSER || itemId == RaceItemId.GROUP_ANGLE || itemId == RaceItemId.STORM)
				{
					humanAnimation[ANIM_USE_ITEM].wrapMode = WrapMode.Once;
					humanAnimation.Stop();
					humanAnimation.Play(ANIM_USE_ITEM);
					playingName = ANIM_USE_ITEM;
				}
				else
				{
					humanAnimation[ANIM_USE_ITEM_BACK].wrapMode = WrapMode.Once;
					humanAnimation.Stop();
					humanAnimation.Play(ANIM_USE_ITEM_BACK);
					playingName = ANIM_USE_ITEM_BACK;
				}
			}
		}

		internal void PlayBigHSpeed(bool revert = false)
		{
			if (humanAnimation != null && animNames != null)
			{
				if (revert)
				{
					if (!animNames.Contains(ANIM_NEOSPEEDUPRESET) || !animNames.Contains(ANIM_DRIVE))
					{
						return;
					}
					humanAnimation[ANIM_NEOSPEEDUPRESET].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_NEOSPEEDUPRESET);
					humanAnimation.PlayQueued(ANIM_DRIVE);
				}
				else
				{
					if (!animNames.Contains(ANIM_NEOSPEEDUP) || !animNames.Contains(ANIM_NEOSPEEDUPHOLD))
					{
						return;
					}
					humanAnimation[ANIM_NEOSPEEDUP].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_NEOSPEEDUPHOLD].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_NEOSPEEDUP);
					humanAnimation.PlayQueued(ANIM_NEOSPEEDUPHOLD);
				}
			}
			playingName = ANIM_NEOSPEEDUPHOLD;
		}

		internal void PlayHSpeed(bool revert = false)
		{
			if (humanAnimation != null && animNames != null)
			{
				if (revert)
				{
					if (!animNames.Contains(ANIM_SPRINTSOVER) || !animNames.Contains(ANIM_DRIVE))
					{
						return;
					}
					humanAnimation[ANIM_SPRINTSOVER].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_DRIVE].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_SPRINTSOVER);
					humanAnimation.PlayQueued(ANIM_DRIVE);
				}
				else
				{
					if (!animNames.Contains(ANIM_SPRINTSSTART) || !animNames.Contains(ANIM_SPRINTSHOLDING))
					{
						return;
					}
					humanAnimation[ANIM_SPRINTSSTART].wrapMode = WrapMode.Once;
					humanAnimation[ANIM_SPRINTSHOLDING].wrapMode = WrapMode.Loop;
					humanAnimation.Play(ANIM_SPRINTSSTART);
					humanAnimation.PlayQueued(ANIM_SPRINTSHOLDING);
				}
			}
			playingName = ANIM_SPRINTSHOLDING;
		}

		internal void PlayPreStart()
		{
			if (humanAnimation != null)
			{
				humanAnimation.Play(animNameMap[AnimStage.PreStart]);
			}
		}

		internal void PlayWin()
		{
			carState.AnimationFlag = AnimStage.Other;
			if (humanAnimation != null)
			{
				humanAnimation.CrossFade(ANIM_HALF_WIN);
			}
		}

		internal void PlayLose()
		{
			carState.AnimationFlag = AnimStage.Other;
			if (humanAnimation != null)
			{
				humanAnimation.CrossFade(animNameMap[AnimStage.OnLose]);
			}
		}

		internal void PlayExtra(string clipname, WrapMode mode = WrapMode.Once, float resetAfter = 1f)
		{
			if (!(humanAnimation != null))
			{
				return;
			}
			if (animNames.Contains(clipname))
			{
				float delay = ((mode == WrapMode.Once) ? humanAnimation[clipname].length : resetAfter);
				humanAnimation[clipname].wrapMode = mode;
				humanAnimation.CrossFade(clipname, humanAnimation[clipname].length / 4f);
				int flag = ++extraFlag;
				carState.AnimationFlag = AnimStage.HighPrior;
				view.CallDelay(delegate
				{
					if (flag == extraFlag)
					{
						carState.AnimationFlag = AnimStage.CommonDrive;
					}
				}, delay);
			}
			else
			{
				Debug.LogWarning("No animationClip names " + clipname);
			}
		}

		private void checkClip()
		{
			if (humanAnimation == null)
			{
				base.Active = false;
				return;
			}
			AnimStage[] array = Enum.GetValues(typeof(AnimStage)) as AnimStage[];
			AnimStage[] array2 = array;
			foreach (AnimStage animStage in array2)
			{
				animNameMap[animStage] = RaceCallback.GetAnimName(this, animStage);
				check(animNameMap[animStage]);
			}
			check(ANIM_LEFTHOLDING);
			check(ANIM_RIGHTHOLDING);
			check(ANIM_USE_ITEM);
			check(ANIM_USE_ITEM_BACK);
			check(ANIM_HALF_WIN);
			check(ANIM_DRIVE);
			check(ANIM_SPRINTSOVER);
			check(ANIM_SPRINTSSTART);
			check(ANIM_SPRINTSHOLDING);
			check(ANIM_LEFTOVER);
			check(ANIM_RIGHTOVER);
			check(ANIM_LEFTSTART);
			check(ANIM_BACKOVER);
			check(ANIM_BACKSTART);
			check(ANIM_BACKHOLDING);
			check(animNameMap[AnimStage.OnPastEndPoint]);
			humanAnimation[ANIM_LEFTHOLDING].wrapMode = WrapMode.Loop;
			humanAnimation[ANIM_RIGHTHOLDING].wrapMode = WrapMode.Loop;
			humanAnimation[ANIM_USE_ITEM].wrapMode = WrapMode.Once;
			humanAnimation.cullingType = AnimationCullingType.BasedOnUserBounds;
			humanAnimation.AddClip(humanAnimation[animNameMap[AnimStage.OnPastEndPoint]].clip, ANIM_HALF_WIN);
			humanAnimation[ANIM_HALF_WIN].wrapMode = WrapMode.Loop;
			humanAnimation.Play(ANIM_DRIVE);
			playingName = ANIM_DRIVE;
		}

		private void check(string name)
		{
			if (!animNames.Contains(name))
			{
				Debug.LogWarning("No AnimationClip names " + name);
			}
		}

		public static bool HasClip(Animation ani, string name)
		{
			if (ani == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			foreach (AnimationState item in ani)
			{
				if (name == item.name)
				{
					return true;
				}
			}
			return false;
		}
	}
}
