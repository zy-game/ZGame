using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillImmuneAttack : CarChipTechnologySkillBase
	{
		private List<RaceItemId> itemList = new List<RaceItemId>
		{
			RaceItemId.UFO,
			RaceItemId.WATER_FLY,
			RaceItemId.ROCKET,
			RaceItemId.MINE,
			RaceItemId.BANANA,
			RaceItemId.INK_BOTTLE,
			RaceItemId.WATER_BOMB
		};

		private float startTime;

		private Transform angelMesh;

		private ItemFollower follower;

		private List<int> ItemList = new List<int>();

		public Float Probability;

		public override CarSkillId ID => CarSkillId.ITEM_IMMUNE_ATTACK_ITEM;

		public override void Init(int param1, int param2, int param3)
		{
			Probability = param2;
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)Probability;
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks2.AffectChecker, new ModifyAction<RaceItemId, bool>(attackChecker));
			}
		}

		private void attackChecker(RaceItemId id, ref bool attackable)
		{
			attackable = !ISImmuneAttack(id);
		}

		private void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if (ISImmuneAttack(id))
			{
				OnImmuneAttack(car, id);
			}
		}

		private void OnImmuneAttack(CarState car, RaceItemId id)
		{
			if (car.CallBacks.OnAffectedByItem != null)
			{
				car.CallBacks.OnAffectedByItem(id, car, ItemCallbackType.DEFEND, this);
			}
			Singleton<ResourceOffer>.Instance.Load("Effects/Sence/dunpai01", delegate(UnityEngine.Object o)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
				if (!(gameObject == null))
				{
					gameObject.transform.parent = car.transform;
					angelMesh = gameObject.transform;
					angelMesh.rotation = car.transform.rotation;
					follower = new ItemFollower(car, angelMesh, 0f, null, null, look: true);
					follower.Start();
				}
			});
			if (RaceAudioManager.ActiveInstance != null)
			{
				car.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_tianshi);
			}
			startTime = Time.time;
			CarView view = car.view;
			view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(update));
		}

		private void update()
		{
			if ((double)(Time.time - startTime) > 1.5)
			{
				if (follower != null)
				{
					follower.finish();
				}
				if ((bool)angelMesh)
				{
					UnityEngine.Object.Destroy(angelMesh.gameObject);
				}
			}
		}

		public override void Toggle()
		{
			Debug.LogError("免疫攻击");
			base.Toggle();
			base.ChipTechnologyToggle();
		}

		public bool ISImmuneAttack(RaceItemId id)
		{
			if (itemList.Contains(id) && Usable() && SkillUsable())
			{
				Toggle();
				return true;
			}
			return false;
		}
	}
}
