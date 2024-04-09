using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	internal class ItemController : ControllerBase
	{
		private CarState carState;

		private CarModel carModel;

		private bool boxLock;

		private GameObject itemboxSpark;

		private bool _ignoreItemBox;

		public bool IgnoreItemBox
		{
			get
			{
				return _ignoreItemBox;
			}
			set
			{
				_ignoreItemBox = value;
			}
		}

		public bool BoxLock
		{
			get
			{
				return boxLock;
			}
			set
			{
				boxLock = value;
			}
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			CarView view = carState.view;
			view.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view.OnTriggerBegin, new Action<Collider>(onCollisionEnter));
			if (active)
			{
				CarView view2 = carState.view;
				view2.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view2.OnTriggerBegin, new Action<Collider>(onCollisionEnter));
			}
		}

		public void Init(CarState state, CarModel model)
		{
			carState = state;
			carModel = model;
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
			CarCallBack callBacks2 = carState.CallBacks;
			callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
		}

		private void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if (car == null || car.view != carState.view || carState.view == null)
			{
				return;
			}
			switch (type)
			{
				case ItemCallbackType.AFFECT:
					if (o != null)
					{
						if (o is RaceItemBase)
						{
							AddApplyItem((RaceItemBase)o);
						}
						else if (o is PassiveTrigger)
						{
							AddApplyTrigger((PassiveTrigger)o);
						}
					}
					break;
				case ItemCallbackType.BREAK:
					if (o != null)
					{
						if (o is RaceItemBase)
						{
							DelApplyItem((RaceItemBase)o);
						}
						else if (o is PassiveTrigger)
						{
							DelApplyTrigger((PassiveTrigger)o);
						}
					}
					break;
			}
		}

		internal virtual void AddItem(RaceItemBase item)
		{
			List<RaceItemBase> items = carState.Items;
			if (items.Count < 2)
			{
				items.Add(item);
				if (carState.CallBacks.OnGetItem != null)
				{
					carState.CallBacks.OnGetItem(item);
				}
			}
		}

		internal virtual int OnUseItem(RaceItemBase item, int itemIndex)
		{
			int index = GetIndex(item, itemIndex);
			List<RaceItemBase> items = carState.Items;
			if (index >= items.Count || index < 0)
			{
				return -1;
			}
			items.RemoveAt(index);
			if (RaceAudioManager.ActiveInstance != null)
			{
				carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_throw);
			}
			return 1;
		}

		internal void ToggleItemAsTarget(RaceItemBase item, ItemParams ps, Action<RaceItemBase> onToggle = null)
		{
			if (RaceItemFactory.ItemNeedEnqued(item.ItemId))
			{
				Queue<RaceItemBase> que = carState.ItemEffectQueue;
				item.AddCallback(delegate(RaceItemBase it, ItemCallbackType type)
				{
					switch (type)
					{
						case ItemCallbackType.BREAK:
							if (que.Count > 0)
							{
								que.Dequeue();
								if (que.Count > 0)
								{
									que.Peek()?.Toggle(ps);
								}
							}
							break;
						case ItemCallbackType.TOGGLE:
							if (onToggle != null)
							{
								onToggle(it);
							}
							break;
					}
				});
				if (que.Count == 0)
				{
					item.Toggle(ps);
				}
				que.Enqueue(item);
			}
			else
			{
				item.Toggle(ps);
			}
		}

		internal virtual void AddApplyItem(RaceItemBase item, bool sameTypeAllowed = true)
		{
			if (carState.ApplyItems.Contains(item))
			{
				LogWarning(carState.view, " Item reinsert. ", item, " iid:", item.IID);
			}
			else if (!sameTypeAllowed && ApplyingItem(item.ItemId))
			{
				LogWarning(carState.view, " Same type Item reinsert. ", item, " iid:", item.IID);
			}
			else
			{
				LinkedList<RaceItemBase> applyItems = carState.ApplyItems;
				applyItems.AddLast(item);
			}
		}

		internal void DelApplyItem(RaceItemBase item)
		{
			carState.ApplyItems.Remove(item);
		}

		internal void AddApplyTrigger(PassiveTrigger trg)
		{
			if (ApplyingTrigger(trg.ItemId))
			{
				LogWarning("This type of trigger already exist.");
			}
			else
			{
				carState.ApplyTriggers.AddLast(trg);
			}
		}

		internal void DelApplyTrigger(PassiveTrigger trg)
		{
			carState.ApplyTriggers.Remove(trg);
		}

		internal bool ApplyingExcept(PassiveTrigger trigger, List<PassiveTrigger> buf = null)
		{
			bool result = false;
			foreach (PassiveTrigger applyTrigger in carState.ApplyTriggers)
			{
				if (applyTrigger.ItemId == trigger.ItemId && applyTrigger != trigger)
				{
					result = true;
					if (buf == null)
					{
						return result;
					}
					buf.Add(applyTrigger);
				}
			}
			return result;
		}

		internal bool ApplyingTrigger(RaceItemId id, List<PassiveTrigger> buf = null)
		{
			bool result = false;
			foreach (PassiveTrigger applyTrigger in carState.ApplyTriggers)
			{
				if (applyTrigger.ItemId == id)
				{
					result = true;
					if (buf == null)
					{
						return result;
					}
					buf.Add(applyTrigger);
				}
			}
			return result;
		}

		internal bool ApplyingTrigger(GameObject triggerObject, List<PassiveTrigger> buf = null)
		{
			bool result = false;
			foreach (PassiveTrigger applyTrigger in carState.ApplyTriggers)
			{
				if (applyTrigger.ItemObject == triggerObject)
				{
					result = true;
					if (buf == null)
					{
						return result;
					}
					buf.Add(applyTrigger);
				}
			}
			return result;
		}

		internal bool ApplyingExcept(RaceItemBase item, List<RaceItemBase> buf = null)
		{
			bool result = false;
			foreach (RaceItemBase applyItem in carState.ApplyItems)
			{
				if (applyItem.ItemId == item.ItemId && applyItem != item)
				{
					result = true;
					if (buf == null)
					{
						return result;
					}
					buf.Add(applyItem);
				}
			}
			return result;
		}

		internal bool ApplyingItem(RaceItemId id, List<RaceItemBase> buf = null)
		{
			bool result = false;
			foreach (RaceItemBase applyItem in carState.ApplyItems)
			{
				if (applyItem.ItemId == id)
				{
					result = true;
					if (buf == null)
					{
						return result;
					}
					buf.Add(applyItem);
				}
				else if (id == RaceItemId.GROUP_GAS && applyItem.ItemId == RaceItemId.GAS && applyItem is CommonGasItem { IsTeam: not false })
				{
					if (buf == null)
					{
						return true;
					}
					result = true;
					buf.Add(applyItem);
				}
			}
			return result;
		}

		internal int GetIndex(RaceItemBase item, int index)
		{
			List<RaceItemBase> items = carState.Items;
			if (index < items.Count && items[index] != null && item.ItemId == items[index].ItemId)
			{
				return index;
			}
			for (int i = 0; i < items.Count; i++)
			{
				if (item.ItemId == items[i].ItemId)
				{
					return i;
				}
			}
			return -1;
		}

		private void onCollisionEnter(Collider col)
		{
			int layer = col.gameObject.layer;
			if (layer == 12 || layer == 15)
			{
				if (layer == 15)
				{
					onToggleItemBox(col);
				}
				else if (col.transform.root.name.StartsWith("Trigger") && carState.CarPlayerType != PlayerType.PLAYER_OTHER)
				{
					Log("On Collide with Trigger.");
					OnTogglePassiveItem(col.transform.root.gameObject, float.MinValue, 0L);
				}
			}
		}

		private void onToggleItemBox(Collider col)
		{
			if (boxLock || _ignoreItemBox)
			{
				return;
			}
			boxLock = true;
			carState.view.CallDelay(delegate
			{
				boxLock = false;
			}, 1f);
			Transform transform = col.transform;
			onItemBoxDismiss(transform.position);
			Vector3 pos = col.transform.localPosition;
			col.transform.localPosition = new Vector3(0f, -9999f, 0f);
			RaceCallback.View.CallDelay_TimeScale(delegate
			{
				if ((bool)col && (bool)col.transform)
				{
					col.transform.localPosition = pos;
				}
			}, RaceConfig.ItemboxResetTime);
			if (carState.CarPlayerType != 0)
			{
				return;
			}
			RaceItemId r = RaceCallback.RandomItemByRank();
			if (r != 0)
			{
				if (carState.CallBacks.OnToggleItemBox != null)
				{
					carState.CallBacks.OnToggleItemBox(carState, ref r);
				}
				RaceItemBase raceItemBase = RaceItemFactory.BuildItemById(r);
				if (raceItemBase == null)
				{
					Debug.LogError($"创建道具失败, id : {r}");
				}
				AddItem(raceItemBase);
				if (carState.CallBacks.OnCrashItemBox != null)
				{
					carState.CallBacks.OnCrashItemBox(raceItemBase.ItemId);
				}
			}
		}

		internal void OnTogglePassiveItem(GameObject go, float dirY = float.MinValue, long playerWorldId = 0L, Action<TriggerData> onToggle = null, Action<PassiveTrigger> onOver = null)
		{
			if (go == null)
			{
				LogWarning("Null name or GameObject.");
				return;
			}
			RaceItemId raceItemId = RaceItemId.NONE;
			ushort num = 0;
			TriggerData triggerData = null;
			SimpleData component = go.GetComponent<SimpleData>();
			if ((bool)component)
			{
				triggerData = component.UserData as TriggerData;
				num = triggerData.InstanceId;
				raceItemId = triggerData.ItemId;
				if (raceItemId == RaceItemId.NONE)
				{
					LogWarning("Error item id");
					return;
				}
				if (triggerData.User == carState && Time.time - triggerData.LayTime < RaceConfig.SelfTriggerInvalidTime)
				{
					LogWarning("Self layout trigger is invalid in ", RaceConfig.SelfTriggerInvalidTime, " seconds.");
					return;
				}
				if (ApplyingTrigger(raceItemId))
				{
					LogWarning("Duplicated trigger invalid.", raceItemId);
					return;
				}
				RaceCallback.SendTriggerToggle(carState, num, raceItemId);
				PassiveTrigger passiveTrigger = RaceItemFactory.BuildTriggerById(raceItemId, num);
				if (passiveTrigger != null)
				{
					StopAllTriggers(passiveTrigger.ItemObject);
					if (onOver != null)
					{
						passiveTrigger.OnOver = (Action<PassiveTrigger>)Delegate.Combine(passiveTrigger.OnOver, onOver);
					}
					passiveTrigger.ItemObject = go;
					ItemParams itemParams = new ItemParams(null, carState, num);
					long num2 = 0L;
					if (dirY != float.MinValue)
					{
						itemParams.DirY = dirY;
						num2 = playerWorldId;
					}
					else
					{
						Vector3 velocity = carState.velocity;
						velocity.y = 0f;
						itemParams.DirY = Quaternion.FromToRotation(Vector3.forward, velocity).eulerAngles.y;
					}
					if (triggerData != null && triggerData.User != null)
					{
						passiveTrigger.User = triggerData.User.view.PlayerInfo.WorldId;
					}
					passiveTrigger.ApplyEffect(itemParams);
					onToggle?.Invoke(triggerData);
				}
				else
				{
					LogWarning("Build Trigger error.");
				}
			}
			else
			{
				LogWarning("No Trigger Object found.");
			}
		}

		internal void StopAllTriggers(GameObject excluding = null)
		{
			LinkedList<PassiveTrigger> linkedList = new LinkedList<PassiveTrigger>(carState.ApplyTriggers);
			foreach (PassiveTrigger item in linkedList)
			{
				if (item != null && (item.ItemObject != excluding || null == excluding))
				{
					item.Stop();
				}
			}
		}

		public void StopAllGasItems()
		{
			if (carState == null || carState.ApplyItems == null)
			{
				return;
			}
			LinkedList<RaceItemBase> linkedList = new LinkedList<RaceItemBase>(carState.ApplyItems);
			foreach (RaceItemBase item in linkedList)
			{
				if (item != null && item.ItemId == RaceItemId.GAS)
				{
					item.Break();
				}
			}
		}

		internal void StopAllItemEffects()
		{
			LinkedList<RaceItemBase> linkedList = new LinkedList<RaceItemBase>(carState.ApplyItems);
			foreach (RaceItemBase item in linkedList)
			{
				item?.Break();
			}
		}

		internal void OnItemEffectStart(bool lockCamera = false)
		{
			switch (carState.CarPlayerType)
			{
				case PlayerType.PLAYER_SELF:
					carState.view.Controller.Active = false;
					if (lockCamera && CameraController.Current != null && CameraController.Current.ViewTarget == carState)
					{
						CameraController.Current.enabled = false;
					}
					if ((bool)carState.rigidbody && !carState.rigidbody.isKinematic)
					{
						carState.rigidbody.velocity = Vector3.zero;
						carState.rigidbody.angularVelocity = Vector3.zero;
					}
					carState.view.pathController.Active = false;
					break;
				case PlayerType.PLAYER_OTHER:
					carState.view.SyncController.Active = false;
					break;
			}
		}

		internal void OnItemEffectEnd(bool freeCamera = true)
		{
			switch (carState.CarPlayerType)
			{
				case PlayerType.PLAYER_SELF:
					carState.view.Controller.Active = true;
					carState.view.pathController.Active = true;
					if (freeCamera && CameraController.Current != null && CameraController.Current.ViewTarget == carState)
					{
						CameraController.Current.enabled = true;
					}
					break;
				case PlayerType.PLAYER_OTHER:
					carState.view.SyncController.Active = true;
					break;
			}
		}

		private void onItemBoxDismiss(Vector3 pos)
		{
			if (RaceAudioManager.ActiveInstance != null)
			{
				carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_eat);
			}
			if (itemboxSpark != null)
			{
				itemboxSpark.SetActive(value: false);
				itemboxSpark.SetActive(value: true);
				return;
			}
			itemboxSpark = new GameObject("itemboxSparkTmp");
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemboxSpark, delegate(UnityEngine.Object ob)
			{
				bool activeSelf = itemboxSpark.activeSelf;
				UnityEngine.Object.Destroy(itemboxSpark);
				if (ob != null && carState.view != null)
				{
					itemboxSpark = UnityEngine.Object.Instantiate(ob) as GameObject;
					Transform transform = itemboxSpark.transform;
					transform.parent = carState.transform;
					transform.localPosition = Vector3.zero;
					if (!activeSelf)
					{
						itemboxSpark.SetActive(value: false);
					}
				}
			});
		}

		internal PassiveTrigger TryGetTrigger(RaceItemId itemId, ushort insId)
		{
			List<PassiveTrigger> list = new List<PassiveTrigger>(2);
			if (ApplyingTrigger(itemId, list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].InstanceId == insId)
					{
						return list[i];
					}
				}
			}
			return null;
		}

		internal RaceItemBase TryGetItem(RaceItemId itemId, ushort insId)
		{
			List<RaceItemBase> list = new List<RaceItemBase>(2);
			if (ApplyingItem(itemId, list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].InstanceId == insId)
					{
						return list[i];
					}
				}
			}
			return null;
		}
	}
}
