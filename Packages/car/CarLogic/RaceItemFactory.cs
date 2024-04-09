using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public static class RaceItemFactory
	{
		private static Dictionary<RaceItemId, RaceItemParameters> _dicItemParameters = new Dictionary<RaceItemId, RaceItemParameters>();

		public static RaceItemId GetIdRandom()
		{
			int num = 18;
			return (RaceItemId)((int)(Time.realtimeSinceStartup * 10000f) % num + 1);
		}

		public static bool ItemNeedEnqued(RaceItemId id)
		{
			return id switch
			{
				RaceItemId.ROCKET => true, 
				RaceItemId.WATER_FLY => true, 
				_ => false, 
			};
		}

		public static bool IdRequire(RaceItemId id)
		{
			return id switch
			{
				RaceItemId.BANANA => true, 
				RaceItemId.CHEAT_BOX => true, 
				RaceItemId.MINE => true, 
				RaceItemId.BLOCK => true, 
				_ => false, 
			};
		}

		public static RaceItemBase BuildItemById(RaceItemId id)
		{
			RaceItemBase result = null;
			switch (id)
			{
				case RaceItemId.ANGEL:
					result = new RaceAngelItem(GetItemParameters(RaceItemId.ANGEL));
					break;
				case RaceItemId.BANANA:
					result = new RaceBananaItem();
					break;
				case RaceItemId.CHEAT_BOX:
					result = new RaceCheatBoxItem(GetItemParameters(RaceItemId.CHEAT_BOX));
					break;
				case RaceItemId.CONTROL_REVERT:
					result = new RaceRevertItem(GetItemParameters(RaceItemId.CONTROL_REVERT));
					break;
				case RaceItemId.FOG:
					result = new RaceFogItem(GetItemParameters(RaceItemId.FOG));
					break;
				case RaceItemId.GAS:
					result = new CommonGasItem(1, 4f, useShake: false);
					break;
				case RaceItemId.GasLevelTwo:
					result = new RaceGasLevelTwo();
					break;
				case RaceItemId.GasLevelThree:
					result = new RaceGasLevelThree();
					break;
				case RaceItemId.GROUP_ANGLE:
					result = new RaceGroupAngelItem(GetItemParameters(RaceItemId.GROUP_ANGLE));
					break;
				case RaceItemId.GROUP_GAS:
					result = new RaceGroupGasItem();
					break;
				case RaceItemId.INK_BOTTLE:
					result = new RaceInkItem(GetItemParameters(RaceItemId.INK_BOTTLE));
					break;
				case RaceItemId.MINE:
					result = new RaceMineItem();
					break;
				case RaceItemId.ROCKET:
					result = new RaceRocketItem();
					break;
				case RaceItemId.UFO:
					result = new RaceUFOItem(GetItemParameters(RaceItemId.UFO));
					break;
				case RaceItemId.UFO_CONFUSER:
					result = new RaceAntiUFOItem();
					break;
				case RaceItemId.WATER_BOMB:
					result = new RaceWaterBombItem(GetItemParameters(RaceItemId.WATER_BOMB));
					break;
				case RaceItemId.WATER_FLY:
					result = new RaceWaterFlyItem();
					break;
				case RaceItemId.BLOCK:
					result = new RaceBlockItem();
					break;
				case RaceItemId.STORM:
					result = new RaceStormItem(GetItemParameters(RaceItemId.STORM));
					break;
				case RaceItemId.COUPLE_GAS:
					result = new CoupleGasItem();
					break;
				case RaceItemId.CUPID_GAS:
					result = new CupidGasItem(1, 4f);
					break;
				case RaceItemId.BOSS1_ROCKET:
					result = new RaceBoss1RocketItem();
					break;
				case RaceItemId.BOSS1_BIG_ROCKET:
					result = new RaceBoss1BigRocketItem();
					break;
				case RaceItemId.BOSS1_BANANA:
					result = new RaceBoss1BananaItem();
					break;
				case RaceItemId.BOSS1_MINE:
					result = new RaceBoss1MineItem();
					break;
				case RaceItemId.BOSS1_FOG:
					result = new RaceBoss1FogItem(GetItemParameters(RaceItemId.BOSS1_FOG));
					break;
				case RaceItemId.BOSS1_WATER_FLY:
					result = new RaceBoss1WaterFlyItem();
					break;
				case RaceItemId.COPS_BLOCK:
					result = new RaceCopsBlockItem();
					break;
				case RaceItemId.COPS_OIL:
					result = new RaceCopsOilItem();
					break;
				case RaceItemId.COPS_CAR:
					result = new RaceCopsCarItem();
					break;
				case RaceItemId.COPS_NAIL:
					result = new RaceCopsNailItem();
					break;
				case RaceItemId.COPS_ELECTRIC:
					result = new RaceCopsElectricItem();
					break;
				case RaceItemId.COPS_COPTER:
					result = new RaceCopsCopterItem();
					break;
			}
			return result;
		}

		public static PassiveTrigger BuildTriggerById(RaceItemId id, ushort instanceId)
		{
			PassiveTrigger result = null;
			switch (id)
			{
				case RaceItemId.BANANA:
					result = new BananaTrigger(instanceId, 0L);
					break;
				case RaceItemId.CHEAT_BOX:
					result = new CheatBoxTrigger(instanceId, 0L);
					break;
				case RaceItemId.MINE:
					result = new MineTrigger(instanceId, 0L);
					break;
				case RaceItemId.WATER_BOMB:
					result = new WaterBombTrigger(instanceId, 0L);
					break;
				case RaceItemId.BLOCK:
					result = new BlockTrigger(instanceId, 0L);
					break;
				case RaceItemId.STORM:
					result = new StormTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_BLOCK:
					result = new CopsBlockTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_CAR:
					result = new CopsCarTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_NAIL:
					result = new CopsNailTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_OIL:
					result = new CopsOilTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_ELECTRIC:
					result = new CopsElectricTrigger(instanceId, 0L);
					break;
				case RaceItemId.COPS_COPTER:
					result = new CopsCopterTrigger(instanceId, 0L);
					break;
			}
			return result;
		}

		public static float GetDelayDuration(RaceItemId id, bool isTrigger = false, CarState target = null, CarState user = null)
		{
			float result = 0f;
			bool flag = false;
			switch (id)
			{
				case RaceItemId.BANANA:
					if (isTrigger)
					{
						result = 3f;
						flag = true;
					}
					break;
				case RaceItemId.CHEAT_BOX:
					if (isTrigger)
					{
						result = 1.5f;
						flag = true;
					}
					break;
				case RaceItemId.MINE:
					if (isTrigger)
					{
						result = 1.5f;
						flag = true;
					}
					break;
				case RaceItemId.ROCKET:
					result = 1.5f;
					flag = true;
					break;
				case RaceItemId.WATER_BOMB:
					if (isTrigger)
					{
						result = 3f;
						flag = true;
					}
					break;
				case RaceItemId.WATER_FLY:
					result = 3f;
					flag = true;
					break;
				case RaceItemId.STORM:
					if (isTrigger)
					{
						result = 3f;
						flag = true;
					}
					break;
				case RaceItemId.BLOCK:
					if (isTrigger)
					{
						result = 3f;
						flag = true;
					}
					break;
			}
			if (flag && _dicItemParameters.ContainsKey(id))
			{
				result = _dicItemParameters[id].AffectedTime;
			}
			return result;
		}

		public static float GetLifeTime(RaceItemId id)
		{
			float result = 0f;
			if (_dicItemParameters.ContainsKey(id))
			{
				result = _dicItemParameters[id].LifeTime;
			}
			return result;
		}

		public static float GetExtraTimeScale(RaceItemId id)
		{
			float result = 1f;
			switch (id)
			{
				case RaceItemId.UFO:
					result = 0.2f;
					break;
				case RaceItemId.CONTROL_REVERT:
					result = 0.6f;
					break;
				case RaceItemId.INK_BOTTLE:
					result = 0.85f;
					break;
				case RaceItemId.GAS:
					result = 1.2f;
					break;
			}
			return result;
		}

		public static bool RequireIcon(RaceItemId id, bool isTrigger = false)
		{
			switch (id)
			{
				case RaceItemId.WATER_BOMB:
					if (!isTrigger)
					{
						return false;
					}
					break;
				case RaceItemId.NONE:
				case RaceItemId.ROCKET:
				case RaceItemId.BANANA:
				case RaceItemId.MINE:
				case RaceItemId.FOG:
				case RaceItemId.CHEAT_BOX:
				case RaceItemId.STORM:
				case RaceItemId.BLOCK:
				case RaceItemId.COPS_BLOCK:
				case RaceItemId.COPS_NAIL:
				case RaceItemId.COPS_CAR:
				case RaceItemId.COPS_OIL:
				case RaceItemId.COPS_HIT:
				case RaceItemId.COPS_COPTER:
				case RaceItemId.COPS_ELECTRIC:
					return false;
			}
			return true;
		}

		public static Vector3 GetLayPosition(RaceItemId id, CarState target, CarState user)
		{
			Vector3 result = Vector3.zero;
			switch (id)
			{
				case RaceItemId.WATER_BOMB:
					if (user != null)
					{
						RaceItemParameters itemParameters2 = GetItemParameters(id);
						float targetDis2 = float.Parse(itemParameters2.Params[0]);
						result = user.view.GetForwardPosition(targetDis2);
					}
					break;
				case RaceItemId.BANANA:
					if (user != null)
					{
						result = user.transform.TransformPoint(new Vector3(0f, 0f, -4f));
					}
					break;
				case RaceItemId.MINE:
					if (user != null)
					{
						result = user.transform.TransformPoint(new Vector3(0f, 0f, -3f));
					}
					break;
				case RaceItemId.CHEAT_BOX:
					if (user != null)
					{
						result = user.transform.TransformPoint(new Vector3(0f, 0f, -3f));
					}
					break;
				case RaceItemId.BLOCK:
					if (user != null)
					{
						result = user.transform.TransformPoint(new Vector3(0f, 0f, -3f));
					}
					break;
				case RaceItemId.STORM:
					if (user != null)
					{
						RaceItemParameters itemParameters = GetItemParameters(id);
						float targetDis = float.Parse(itemParameters.Params[0]);
						result = user.view.GetForwardPosition(targetDis);
					}
					break;
				default:
					if (user != null)
					{
						result = user.transform.TransformPoint(new Vector3(0f, 0f, -2f));
					}
					break;
			}
			return result;
		}

		public static void SetSmallViewCamera(CameraController cc, RaceItemId id)
		{
			if (!(cc == null) && cc.ViewTarget != null)
			{
				cc.ViewCamera.tag = "Untagged";
				if (RacePathManager.ActiveInstance != null)
				{
					cc.SetSkybox(RacePathManager.ActiveInstance.SkyboxMaterial);
				}
				if (id == RaceItemId.BANANA || id == RaceItemId.UFO || id == RaceItemId.CONTROL_REVERT || id == RaceItemId.INK_BOTTLE)
				{
					Vector3 localPosition = cc.ViewCamera.transform.localPosition;
					localPosition.Set(0f, 2f, 7.12f);
					cc.ViewCamera.transform.localPosition = localPosition;
					localPosition = cc.ViewCamera.transform.localEulerAngles;
					localPosition.Set(14.6f, 180f, 0f);
					cc.ViewCamera.transform.localEulerAngles = localPosition;
					cc.rotationDamping = 100f;
					cc.PosDamping = 1f;
					cc.UpdateCamera();
					cc.rotationDamping = 0f;
				}
				else
				{
					cc.enabled = false;
					cc.transform.position = cc.ViewTarget.transform.TransformPoint(new Vector3(0f, 0.5f, 3.5f));
					cc.transform.LookAt(cc.ViewTarget.transform.position + new Vector3(0f, 0.5f, 0f));
				}
			}
		}

		public static bool AttackIconRequire(RaceItemId id)
		{
			return id switch
			{
				RaceItemId.ROCKET => true, 
				RaceItemId.WATER_FLY => true, 
				RaceItemId.INK_BOTTLE => true, 
				_ => false, 
			};
		}

		public static void SetItemParameters(RaceItemId id, RaceItemParameters para)
		{
			_dicItemParameters[id] = para;
		}

		public static RaceItemParameters GetItemParameters(RaceItemId id)
		{
			if (_dicItemParameters.ContainsKey(id))
			{
				return _dicItemParameters[id];
			}
			Debug.LogError($"道具参数没找到，id : {id}");
			return new RaceItemParameters();
		}
	}
}
