using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemMover : ItemTweener
	{
		protected Transform _controlTsf;

		protected Vector3 _direction;

		protected float _speed;

		public ItemMover(CarState target, Transform controlTsf, Vector3 dir, float speed, float duration = 3f, Action callback = null)
			: base(target, duration, callback)
		{
			_controlTsf = controlTsf;
			_direction = dir.normalized;
			_speed = speed;
		}

		public override void Start()
		{
			base.Start();
			AbstractView abstractView = _controlTsf.GetComponent<AbstractView>();
			if (abstractView == null)
			{
				abstractView = _controlTsf.gameObject.AddComponent<AbstractView>();
			}
			AbstractView abstractView2 = abstractView;
			abstractView2.OnFixedupdate = (Action)Delegate.Combine(abstractView2.OnFixedupdate, new Action(Update));
		}

		protected void Update()
		{
			if (Time.time - startTime > maxDuration)
			{
				finish();
			}
			_controlTsf.transform.position += _speed * Time.fixedDeltaTime * _direction;
		}

		public override void finish()
		{
			base.finish();
			AbstractView component = _controlTsf.GetComponent<AbstractView>();
			if (component != null)
			{
				component.OnFixedupdate = (Action)Delegate.Remove(component.OnFixedupdate, new Action(Update));
			}
		}
	}
}
