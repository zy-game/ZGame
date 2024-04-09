using System;
using UnityEngine;

namespace CarLogic
{
	public class RaceWaterFlyItem : RaceItemBase
	{
		private Transform flyTransform;

		private ItemTranslator translator;

		private ItemFloter floater;

		private Transform child;

		private GameObject _splash;

		private const float shakeInterval = 0.35f;

		private const float shakeRange = 1.2f;

		private float shakeStart;

		private float sign = 1f;

		private float upGoDuration = 0.25f;

		private Vector3 v;

		private Vector3 spos;

		public override RaceItemId ItemId => RaceItemId.WATER_FLY;

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (targets == null || targets.Length < 1)
			{
				base.Toggle(ps);
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemWaterFly, delegate(UnityEngine.Object o)
				{
					if (!(o == null) && itemParams != null && itemParams.user != null && !(itemParams.user.transform == null))
					{
						GameObject bg2 = UnityEngine.Object.Instantiate(o) as GameObject;
						setupFly(ps, bg2);
						if (flyTransform == null)
						{
							Debug.LogWarning("Fly Transform == null");
						}
						else
						{
							translator = new ItemTranslator(null, flyTransform, destroyItem: true);
							translator.UpGoTime = upGoDuration;
							translator.FloatOffset = Vector3.up;
							if (itemParams.user != null && itemParams.user.transform != null)
							{
								translator.User = itemParams.user.transform;
							}
							translator.Start();
						}
					}
				});
				Break();
				return;
			}
			if (targets[0].view.ItController.ApplyingItem(RaceItemId.WATER_FLY))
			{
				base.Toggle(ps);
				Break();
				return;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemWaterFly, delegate(UnityEngine.Object o)
			{
				if (!(o == null) && itemParams != null && itemParams.user != null && !(itemParams.user.transform == null))
				{
					GameObject bg = UnityEngine.Object.Instantiate(o) as GameObject;
					setupFly(ps, bg);
					translator = new ItemTranslator(targets[0], flyTransform, destroyItem: false, startFloat);
					translator.UpGoTime = upGoDuration;
					translator.FloatOffset = Vector3.up;
					translator.MinDistance = 4f;
					translator.User = itemParams.user.transform;
					translator.Start();
				}
			});
		}

		private void setupFly(ItemParams ps, GameObject bg)
		{
			if (ps.user != null && !(ps.user.transform == null))
			{
				GameObject gameObject = new GameObject("WaterFly");
				flyTransform = gameObject.transform;
				flyTransform.position = ps.user.transform.position;
				child = bg.transform;
				child.parent = flyTransform;
				child.localPosition = Vector3.zero;
				AbstractView abstractView = gameObject.AddComponent<AbstractView>();
				abstractView.OnUpdate = (Action)Delegate.Combine(abstractView.OnUpdate, new Action(flyShaker));
				flyShaker();
				if (RaceAudioManager.ActiveInstance != null)
				{
					AudioSource audioSource = gameObject.AddComponent<AudioSource>();
					audioSource.clip = RaceAudioManager.ActiveInstance.Sound_shuicangying;
					audioSource.loop = true;
					audioSource.Play();
				}
			}
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		private void flyShaker()
		{
			if (child != null)
			{
				if (Time.time - shakeStart > 0.35f)
				{
					shakeStart = Time.time;
					spos = child.localPosition;
					v = child.localPosition;
					v.x = UnityEngine.Random.Range(0.24000001f, 1.2f) * sign;
					v.y = UnityEngine.Random.Range(0f, 0.6f);
					child.localPosition = v;
					sign = 0f - sign;
				}
				else
				{
					child.localPosition = Vector3.Lerp(spos, v, (Time.time - shakeStart) / 0.35f);
				}
			}
		}

		private void startFloat()
		{
			if (targets != null && targets.Length != 0 && targets[0].transform != null)
			{
				if ((bool)flyTransform)
				{
					UnityEngine.Object.Destroy(flyTransform.gameObject);
				}
				if (targets[0].ReachedEnd)
				{
					Break();
					return;
				}
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemWaterSplash, delegate(UnityEngine.Object o)
				{
					if (o == null || targets[0].transform == null || status == RaceItemStatus.NONE)
					{
						Break();
					}
					else
					{
						_splash = UnityEngine.Object.Instantiate(o) as GameObject;
						_splash.transform.position = targets[0].transform.position;
						ItemFollower itemFollower = new ItemFollower(targets[0], _splash.transform, 0f);
						itemFollower.Start();
					}
				});
				RaceCallback.View.CallDelay(delegate
				{
					if (targets[0] == null || targets[0].transform == null || status == RaceItemStatus.NONE)
					{
						Break();
					}
					else
					{
						bool r = true;
						if (targets[0].CallBacks.AffectChecker != null)
						{
							targets[0].CallBacks.AffectChecker(ItemId, ref r);
						}
						if (targets[0].CallBacks.OnAffectedByItem != null)
						{
							targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.CHECK, this);
						}
						if (!r)
						{
							Break();
						}
						else
						{
							floater = new ItemFloter(targets[0], RaceItemBase.floatCurve, RaceConfig.ItemWaterBubble, endFloat, RaceItemFactory.GetDelayDuration(ItemId));
							floater.Start();
							if (carState != null)
							{
								CarCallBack callBacks = carState.CallBacks;
								callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
							}
							if (targets[0].CallBacks.OnAffectedByItem != null)
							{
								targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
							}
							targets[0].view.CallDelay(delegate
							{
								if (status != 0)
								{
									CarView view = targets[0].view;
									view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(update));
								}
							}, 1f);
							if (CallBack != null)
							{
								CallBack(this, ItemCallbackType.AFFECT);
							}
						}
					}
				}, 0.05f);
				carState = targets[0];
				if (carState.CallBacks.OnFreezeMoveEffect != null)
				{
					carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId));
				}
				if (RaceAudioManager.ActiveInstance != null)
				{
					carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_shuipaojizhong);
				}
			}
			else
			{
				Break();
			}
		}

		private void endFloat()
		{
			Break();
		}

		private void update()
		{
			if (carState == null || (floater != null && Time.time - floater.StartTime < RaceConfig.MinFloatTime) || carState.CallBacks.ItemPauseChecker == null)
			{
				return;
			}
			bool r = false;
			carState.CallBacks.ItemPauseChecker(ItemId, ref r);
			if (r)
			{
				manualPause = true;
				if (floater != null && !floater.Finished)
				{
					floater.finish();
				}
				else
				{
					Break();
				}
			}
		}

		public override void Break()
		{
			base.Break();
			if (translator != null && !translator.Finished)
			{
				translator.finish();
			}
			if (floater != null && !floater.Finished)
			{
				floater.finish();
			}
			if (null != flyTransform)
			{
				UnityEngine.Object.Destroy(flyTransform.gameObject);
			}
			if (null != _splash)
			{
				UnityEngine.Object.Destroy(_splash);
			}
			if (targets != null && targets.Length >= 1 && carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
				if (carState.view != null)
				{
					CarView view = carState.view;
					view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
				}
			}
		}
	}
}
