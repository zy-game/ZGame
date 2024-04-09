using System;
using UnityEngine;

namespace CarLogic
{
	public class RaceRocketItem : RaceItemBase
	{
		private Transform rocketTransform;

		private GameObject _explosion;

		private ParticleSystem[] _particles;

		private ItemTranslator translator;

		private ItemThrower thrower;

		private const float upGoDuration = 0f;

		private float startSpeed = 5f;

		private float throwDuration;

		private float throwingTime;

		private Vector3 _floatOffset = new Vector3(0f, 0.3f, 1.2f);

		public override RaceItemId ItemId => RaceItemId.ROCKET;

		public override void SendRequest(ItemParams ps)
		{
			base.SendRequest(ps);
		}

		public override bool Usable(ItemParams ps)
		{
			return base.Usable(ps);
		}

		public static bool Available(CarState[] targets, CarState user, params object[] objs)
		{
			return true;
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			ToggleEffect(ps);
		}

		public override void ToggleEffect(ItemParams ps)
		{
			targets = ps.targets;
			CarState target = null;
			base.ToggleEffect(ps);
			if (targets != null && targets.Length != 0)
			{
				target = targets[0];
				carState = target;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemRocket, delegate(UnityEngine.Object o)
			{
				if (!(o == null) && ps.user != null && !(ps.user.transform == null) && status != 0)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					status = RaceItemStatus.FLYING;
					rocketTransform = gameObject.transform;
					_particles = rocketTransform.GetComponentsInChildren<ParticleSystem>();
					rocketTransform.position = ps.user.transform.TransformPoint(_floatOffset);
					rocketTransform.rotation = Quaternion.identity;
					translator = new ItemTranslator(target, rocketTransform, destroyItem: false, startThrow);
					translator.FloatOffset = _floatOffset;
					translator.User = ((ps.user == null) ? null : ps.user.transform);
					if (target != null)
					{
						PauseAllParticle();
						CarView view = target.view;
						view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(Throwing));
					}
					translator.UpGoTime = 0f;
					translator.FloatTime = 0.1f;
					translator.StartSpeed = startSpeed;
					if (RaceAudioManager.ActiveInstance != null)
					{
						if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
						{
							AudioSource audioSource = gameObject.AddComponent<AudioSource>();
							audioSource.clip = RaceAudioManager.ActiveInstance.Sound_daodanfly;
							audioSource.Play();
						}
						else if (ps.user != null && ps.user.CarPlayerType == PlayerType.PLAYER_SELF)
						{
							AudioSource audioSource2 = gameObject.AddComponent<AudioSource>();
							audioSource2.clip = RaceAudioManager.ActiveInstance.Sound_daodanfashe;
							audioSource2.Play();
						}
					}
				}
			});
		}

		private void PauseAllParticle()
		{
			if (_particles != null && _particles.Length != 0)
			{
				for (int i = 0; i < _particles.Length; i++)
				{
					_particles[i].Pause(withChildren: true);
				}
			}
		}

		private void PlayAllParticle()
		{
			if (_particles == null || _particles.Length == 0)
			{
				return;
			}
			for (int i = 0; i < _particles.Length; i++)
			{
				if (_particles != null)
				{
					_particles[i].Play(withChildren: true);
				}
			}
		}

		private void Throwing()
		{
			throwingTime += Time.deltaTime;
			if (base.Params != null && base.Params.user != null && null != rocketTransform)
			{
				rocketTransform.rotation = base.Params.user.transform.rotation;
				rocketTransform.position = base.Params.user.transform.TransformPoint(_floatOffset);
			}
			if (throwingTime >= throwDuration)
			{
				CarView view = targets[0].view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(Throwing));
				PlayAllParticle();
				translator.Start();
			}
		}

		public override void ApplyData(ItemParams ps)
		{
			base.ApplyData(ps);
		}

		public override void Break()
		{
			if (rocketTransform != null)
			{
				UnityEngine.Object.Destroy(rocketTransform.gameObject);
			}
			if (null != _explosion)
			{
				UnityEngine.Object.Destroy(_explosion);
			}
			if (targets != null && targets.Length != 0)
			{
				if (thrower != null)
				{
					thrower.finish();
				}
				if (targets[0] != null && targets[0].view != null)
				{
					CarView view = targets[0].view;
					view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(Throwing));
					targets[0].view.ItController.OnItemEffectEnd();
				}
				base.Break();
			}
		}

		private void throwEnd()
		{
			Break();
		}

		private void startThrow()
		{
			if (targets == null || targets.Length == 0)
			{
				Break();
				return;
			}
			if ((bool)rocketTransform)
			{
				UnityEngine.Object.Destroy(rocketTransform.gameObject);
			}
			if (targets[0].ReachedEnd || targets[0].view == null || status == RaceItemStatus.NONE)
			{
				Break();
				return;
			}
			bool r = true;
			if (targets[0].CallBacks.AffectChecker != null)
			{
				targets[0].CallBacks.AffectChecker(ItemId, ref r);
			}
			if (targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
			{
				targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.CHECK, this);
			}
			if (!r)
			{
				Break();
				return;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemExplosion, delegate(UnityEngine.Object o)
			{
				if (!(o == null) && !(targets[0].view == null) && status != 0)
				{
					_explosion = UnityEngine.Object.Instantiate(o) as GameObject;
					if (targets[0] != null && targets[0].transform != null)
					{
						CarState carState = targets[0];
						Vector3 vector = carState.LastNormal;
						Vector3 position = carState.transform.position;
						if (Physics.Raycast(position + vector * 3f, -vector, out var hitInfo, 100f, 256))
						{
							vector = hitInfo.normal;
						}
						_explosion.transform.position = position;
						_explosion.transform.rotation = Quaternion.LookRotation(carState.transform.forward, vector);
						if (RaceAudioManager.ActiveInstance != null)
						{
							AudioSource audioSource = _explosion.AddComponent<AudioSource>();
							audioSource.clip = RaceAudioManager.ActiveInstance.Sound_daodanjizhong;
							audioSource.Play();
						}
					}
					if (targets != null && targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
					{
						targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
					}
				}
			});
			targets[0].view.ItController.OnItemEffectStart();
			thrower = new ItemThrower(targets[0], RaceItemFactory.GetDelayDuration(ItemId), throwEnd, ItemId);
			status = RaceItemStatus.APPLYING;
			thrower.Start();
			if (CallBack != null)
			{
				CallBack(this, ItemCallbackType.AFFECT);
			}
			carState = targets[0];
			if (carState.CallBacks.OnFreezeMoveEffect != null)
			{
				carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId));
			}
		}
	}
}
