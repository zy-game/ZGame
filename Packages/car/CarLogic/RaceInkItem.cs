using System;
using UnityEngine;

namespace CarLogic
{
	public class RaceInkItem : RaceItemBase
	{
		private ItemTranslator translator;

		private float duration = 4f;

		private const float upGoDuration = 0.25f;

		private ParticleSystem[] _particles;

		private Animator[] _animators;

		private Vector3 _floatOffset = Vector3.up;

		private GameObject _inkEffect;

		public override RaceItemId ItemId => RaceItemId.INK_BOTTLE;

		public RaceInkItem(RaceItemParameters param)
		{
			duration = param.AffectedTime;
		}

		private void StartTranslator(Transform inkTransform, CarState target, Action callback = null, float duration = 3f)
		{
			PauseAllParticles();
			translator = new ItemTranslator(targets[0], inkTransform, destroyItem: true, startSpill, 2f);
			translator.FloatOffset = _floatOffset;
			translator.UpGoTime = 0.25f;
			translator.User = itemParams.user.transform;
			translator.Start();
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (targets == null || targets.Length < 1 || !targets[0].view.gameObject.activeSelf)
			{
				Debug.LogWarning("Null Targets");
				return;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemInkBottle, delegate(UnityEngine.Object o)
			{
				if (!(targets[0].view == null) && status != 0 && targets[0].view.gameObject.activeSelf)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					if (!(gameObject == null))
					{
						_particles = gameObject.GetComponentsInChildren<ParticleSystem>();
						_animators = gameObject.GetComponentsInChildren<Animator>();
						Transform transform = gameObject.transform;
						transform.localRotation = Quaternion.identity;
						if (targets == null || targets.Length < 1 || ps.user == null)
						{
							Debug.LogWarning(string.Concat("Null Target or User t:", targets, " u:", ps.user));
							StartTranslator(transform, null);
							Break();
						}
						else
						{
							StartTranslator(transform, targets[0], startSpill, 2f);
						}
					}
				}
			});
		}

		private void PauseAllParticles()
		{
			if (_particles != null && _particles.Length != 0)
			{
				for (int i = 0; i < _particles.Length; i++)
				{
					_particles[i].Pause(withChildren: true);
				}
			}
		}

		private void PlayAllParticles()
		{
			if (_particles != null && _particles.Length != 0)
			{
				for (int i = 0; i < _particles.Length; i++)
				{
					_particles[i].Play(withChildren: true);
				}
			}
		}

		public static GameObject LoadInkEffect()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(RaceConfig.ItemInkEffect)) as GameObject;
			UnityEngine.Object.Destroy(gameObject, 4f);
			return gameObject;
		}

		private void startSpill()
		{
			if (targets == null || targets.Length < 1 || !targets[0].view.gameObject.activeSelf)
			{
				Break();
				Debug.LogWarning("No Target.");
			}
			else
			{
				if (targets[0].view == null)
				{
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
				if (targets[0].CarPlayerType != 0)
				{
					if (targets != null && targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
					{
						targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
					}
					targets[0].view.CallDelay(Break, duration);
					Debug.Log("Not self.");
					return;
				}
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemInkEffect, delegate(UnityEngine.Object o)
				{
					if (status != 0 && targets[0].view.gameObject.activeSelf)
					{
						_inkEffect = UnityEngine.Object.Instantiate(o) as GameObject;
						if (!(_inkEffect == null))
						{
							Transform transform = findContainer();
							if (transform != null)
							{
								Transform transform2 = _inkEffect.transform;
								transform2.parent = transform;
								transform2.localPosition = new Vector3(0f, 0f, 0f);
								transform2.localRotation = Quaternion.identity;
								if (RaceAudioManager.ActiveInstance != null)
								{
									AudioSource audioSource = _inkEffect.AddComponent<AudioSource>();
									audioSource.clip = RaceAudioManager.ActiveInstance.Sound_moshui;
									audioSource.Play();
								}
								if (targets != null && targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
								{
									targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
								}
							}
							targets[0].view.CallDelay(Break, duration);
						}
					}
				});
			}
		}

		private static Transform findContainer()
		{
			LayerMask layerMask = LayerMask.NameToLayer("UI");
			Camera[] allCameras = Camera.allCameras;
			for (int i = 0; i < allCameras.Length; i++)
			{
				if (allCameras[i].gameObject.layer == layerMask.value && allCameras[i].gameObject.name.Contains("RaceCamera"))
				{
					return allCameras[i].transform;
				}
			}
			return null;
		}

		public override void Break()
		{
			if (translator != null && !translator.Finished)
			{
				translator.finish();
			}
			if (_inkEffect != null)
			{
				UnityEngine.Object.Destroy(_inkEffect);
			}
			base.Break();
		}
	}
}
