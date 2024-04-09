using System;
using UnityEngine;

namespace CarLogic
{
	public class RaceCopsCopterItem : RaceCopsItemBase
	{
		protected RaceItemParameters _parameter;

		protected float _rocketPosOffset;

		protected float _rocketInterval = 1f;

		protected float _copterRotate = 1f;

		protected float _rocketLife = 1f;

		protected float _copterFlySpeed = 1f;

		protected float _lastThrowTime = float.MinValue;

		protected float _velocitySmooth;

		protected const string PreObjPath = "Effects/Sence/CopsPreCopter";

		protected Transform tsfCopter;

		protected CarState _target;

		protected ItemNodeTranslator _translator;

		protected override string objName => "TriggerCopter";

		protected override string objPath => RaceConfig.CopsItemCopter;

		public override RaceItemId ItemId => RaceItemId.COPS_COPTER;

		public override void Toggle(ItemParams ps)
		{
			BaseToggle(ps);
			_target = ps.targets[0];
			if (_target == null || _target.view == null)
			{
				Break();
				return;
			}
			Singleton<ResourceOffer>.Instance.Load("Effects/Sence/CopsPreCopter", delegate(UnityEngine.Object obj)
			{
				if (!(obj == null) && _target != null && !(_target.view == null) && status != 0)
				{
					_parameter = RaceItemFactory.GetItemParameters(ItemId);
					_rocketLife = float.Parse(_parameter.Params[0]);
					_copterFlySpeed = float.Parse(_parameter.Params[1]);
					_rocketPosOffset = float.Parse(_parameter.Params[2]);
					_rocketInterval = float.Parse(_parameter.Params[3]);
					_copterRotate = float.Parse(_parameter.Params[4]);
					GameObject gameObject = UnityEngine.Object.Instantiate(obj) as GameObject;
					status = RaceItemStatus.FLYING;
					tsfCopter = gameObject.transform;
					tsfCopter.position = _target.transform.position;
					Animation componentInChildren = gameObject.GetComponentInChildren<Animation>();
					componentInChildren[componentInChildren.clip.name].speed = 10f / (float)_parameter.LifeTime;
					AbstractView abstractView = gameObject.GetComponent<AbstractView>();
					if (abstractView == null)
					{
						abstractView = gameObject.AddComponent<AbstractView>();
					}
					AbstractView abstractView2 = abstractView;
					abstractView2.OnUpdate = (Action)Delegate.Combine(abstractView2.OnUpdate, new Action(ThrowRocket));
					ItemNodeTranslator itemNodeTranslator = new ItemNodeTranslator(_target, gameObject.transform, _target.LinearVelocity, _copterRotate, _parameter.LifeTime, Finish);
					itemNodeTranslator.Start();
					_lastThrowTime = Time.realtimeSinceStartup;
					_translator = itemNodeTranslator;
					if (RaceAudioManager.ActiveInstance != null)
					{
						AudioSource audioSource = gameObject.AddComponent<AudioSource>();
						audioSource.clip = RaceAudioManager.ActiveInstance.Sound_Airplane;
						audioSource.loop = true;
						audioSource.maxDistance = 20f;
						audioSource.minDistance = 10f;
						audioSource.Play();
					}
				}
			});
		}

		protected Vector3 GetPosition(CarState carState, float disZ, float disX)
		{
			float num = UnityEngine.Random.Range(-1f, 1f);
			float num2 = UnityEngine.Random.Range(0.5f, 1f);
			return carState.transform.position + carState.transform.forward * disZ * num2 + carState.transform.right * disX * num;
		}

		protected void ThrowRocket()
		{
			if (_target == null || null == _target.transform)
			{
				return;
			}
			_translator.FlySpeed = Mathf.SmoothDamp(_translator.FlySpeed, _target.LinearVelocity, ref _velocitySmooth, 0.5f);
			if (!(Time.realtimeSinceStartup - _lastThrowTime > _rocketInterval))
			{
				return;
			}
			Transform transform = _target.transform;
			if (null == transform)
			{
				return;
			}
			_lastThrowTime = Time.realtimeSinceStartup;
			Vector3 targetPosition = GetPosition(_target, _target.LinearVelocity, _rocketPosOffset);
			Ray ray = new Ray(targetPosition + transform.up * 5f, -transform.up);
			if (!Physics.Raycast(ray, out var hit, 10f, LayerMask.GetMask("Road")))
			{
				return;
			}
			Singleton<ResourceOffer>.Instance.Load(objPath, delegate(UnityEngine.Object obj)
			{
				GameObject go = UnityEngine.Object.Instantiate(obj) as GameObject;
				go.transform.position = hit.point;
				Vector3 up = tsfCopter.transform.position + tsfCopter.transform.up * 20f - targetPosition;
				go.transform.up = up;
				Transform transform2 = go.transform.Find("shadow");
				if (transform2 != null)
				{
					transform2.up = hit.normal;
				}
				Transform transform3 = go.transform.Find("paopao");
				if (transform3 != null)
				{
					transform3.up = hit.normal;
				}
				go.name = objName;
				AbstractView abstractView = go.GetComponent<AbstractView>();
				if (abstractView == null)
				{
					abstractView = go.AddComponent<AbstractView>();
				}
				abstractView.CallDelay(delegate
				{
					UnityEngine.Object.Destroy(go);
				}, _rocketLife);
				SimpleData simpleData = go.AddComponent<SimpleData>();
				TriggerData data = (TriggerData)(simpleData.UserData = new TriggerData
				{
					LayTime = Time.time,
					ItemId = ItemId,
					InstanceId = itemParams.instanceId,
					User = itemParams.user,
					TriggerObject = go
				});
				saveToBuffer(data);
				addReleaseCallback(simpleData, data);
			});
		}

		public void Finish()
		{
			if (tsfCopter != null)
			{
				UnityEngine.Object.Destroy(tsfCopter.gameObject);
			}
			tsfCopter = null;
			Break();
		}
	}
}
