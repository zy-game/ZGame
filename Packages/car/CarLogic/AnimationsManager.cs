using UnityEngine;

namespace CarLogic
{
	public class AnimationsManager
	{
		private static AnimationsManager _instance;

		private AnimationsInfoBase _configInfo;

		public AnimationsInfoBase ConfigInfo
		{
			get
			{
				return _configInfo;
			}
			set
			{
				_configInfo = value;
				if (_configInfo != null && !_configInfo.IsInit)
				{
					_configInfo.Init();
				}
			}
		}

		public static AnimationsManager GetInstance()
		{
			if (_instance == null)
			{
				_instance = new AnimationsManager();
			}
			return _instance;
		}

		public AnimationsConfigContainer GetAnimationsConfigContainer(string key, CarModelType carModelType = CarModelType.Car)
		{
			if (_configInfo != null)
			{
				return _configInfo.GetAnimationsConfig(key, carModelType);
			}
			return null;
		}

		private void AssignmentNames(AnimationController controller, string key, CarModelType carModelType = CarModelType.Car)
		{
			AnimationsConfigContainer animationsConfig = _configInfo.GetAnimationsConfig(key, carModelType);
			if (animationsConfig != null)
			{
				controller.ANIM_USE_ITEM = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingUseItem, 0);
				controller.ANIM_USE_ITEM_BACK = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingUseItemBack, 0);
				controller.ANIM_DRIVE = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingSit, 0);
				controller.ANIM_BACKOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnBackReset, 0);
				controller.ANIM_BACKSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnBack, 0);
				controller.ANIM_BACKHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnBackHold, 0);
				controller.ANIM_LEFTOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnLeftReset, 0);
				controller.ANIM_LEFTSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnLeft, 0);
				controller.ANIM_LEFTHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnLeftHold, 0);
				controller.ANIM_RIGHTOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnRightReset, 0);
				controller.ANIM_RIGHTSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnRight, 0);
				controller.ANIM_RIGHTHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTurnRightHold, 0);
				controller.ANIM_LEFTDRIFTOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingLeftDriftReset, 0);
				controller.ANIM_LEFTDRIFTSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingLeftDrift, 0);
				controller.ANIM_LEFTDRIFTHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingLeftDriftHold, 0);
				controller.ANIM_RIGHTDRIFTOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingRightDriftReset, 0);
				controller.ANIM_RIGHTDRIFTSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingRightDrift, 0);
				controller.ANIM_RIGHTDRIFTHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingRightDriftHold, 0);
				controller.ANIM_SPRINTSOVER = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingSpeedUpReset, 0);
				controller.ANIM_SPRINTSSTART = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingSpeedUp, 0);
				controller.ANIM_SPRINTSHOLDING = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingSpeedUpHold, 0);
				controller.ANIM_NEOSPEEDUP = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingN2OSpeedUp, 0);
				controller.ANIM_NEOSPEEDUPHOLD = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingN2OSpeedUpHold, 0);
				controller.ANIM_NEOSPEEDUPRESET = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingN2OSpeedUpReset, 0);
				controller.ANIM_LOSE = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingLoseSit, 0);
				controller.ANIM_HALF_WIN = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingVictorySit, 0);
				controller.ANIM_OVERTAKE = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingOverTake, 0);
				controller.ANIM_OVERTAKEN = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingBeOverTake, 0);
				controller.ANIM_DEVIL = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingDirectionChaos, 0);
				controller.ANIM_BUBBLE = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingBubble, 0);
				controller.ANIM_UFO = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingUFO, 0);
				controller.ANIM_USE_ITEM_SUCCESS = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingUseItemSuccess, 0);
				controller.ANIM_CRASHED = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingCrash, 0);
				controller.ANIM_AIR_JUMPS = animationsConfig.GetAnimations(AnimationKeys.RacingAirJump).ToArray();
				controller.ANIM_THE_FIRST = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTheFirst, 0);
				controller.ANIM_THE_LAST = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingTheLast, 0);
				controller.ANIM_CAR_SHOW = animationsConfig.GetAnimaionByIndex(AnimationKeys.RacingCarShow, 0);
				controller.ANIM_RACE_READY = animationsConfig.GetAnimations(AnimationKeys.RacingReady).ToArray();
			}
		}

		public string GetAnimationsKey(Transform obj)
		{
			if (null != obj)
			{
				SkinnedMeshRenderer component = obj.GetComponent<SkinnedMeshRenderer>();
				if (null != component)
				{
					for (int i = 0; i < component.sharedMaterials.Length; i++)
					{
						if (component.sharedMaterials[i].mainTexture.name.Contains("body"))
						{
							return component.sharedMaterials[i].mainTexture.name;
						}
					}
				}
				Debug.LogWarning($"{obj.name} 中找不到 *_body 材质");
			}
			return null;
		}

		public void ReInitAnimationNames(AnimationController controller, Transform characterObj, CarModelType carModelType)
		{
			if (_configInfo != null && null != characterObj)
			{
				string animationsKey = GetAnimationsKey(characterObj);
				AssignmentNames(controller, animationsKey, carModelType);
			}
		}
	}
}
