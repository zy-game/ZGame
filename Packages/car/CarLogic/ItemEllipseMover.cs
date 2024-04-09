using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemEllipseMover : ItemTweener
	{
		private float _angleSpeed;

		private float _horizontal;

		private float _vertical;

		private AbstractView _view;

		private Transform _item;

		private float _startTime;

		private Vector3 _origionPos;

		private Vector3 _target;

		private float _smootTime = 0.3f;

		private Vector3 _smootVector;

		private RaycastHit _hit;

		private int _roadMask = LayerMask.NameToLayer("Road");

		public ItemEllipseMover(CarState state, Transform itemObject, float horizontal, float vertical, float lineSpeed, Action callback = null, float duration = float.MaxValue)
			: base(state, duration, callback)
		{
			_item = itemObject;
			_angleSpeed = lineSpeed / horizontal;
			_horizontal = horizontal * 0.5f;
			_vertical = vertical * 0.5f;
		}

		public override void Start()
		{
			base.Start();
			if (_item == null)
			{
				Debug.LogWarning("Null item");
				finish();
				return;
			}
			_view = RaceCallback.View;
			AbstractView view = _view;
			view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(update));
			_startTime = Time.realtimeSinceStartup;
			_origionPos = _item.position;
			float num = 0f - _vertical;
			_item.position = _origionPos + _item.forward * num;
		}

		private void update()
		{
			if (_item == null)
			{
				finish();
				return;
			}
			float num = Time.realtimeSinceStartup - _startTime;
			float num2 = -90f + num * _angleSpeed;
			float f = (float)Math.PI / 180f * num2;
			float num3 = _horizontal * Mathf.Cos(f);
			float num4 = _vertical * Mathf.Sin(f);
			_target = _origionPos + _item.forward * num4 + _item.right * num3;
			if (Physics.Raycast(_target + _item.up * 1f, -_item.up, out _hit, _roadMask))
			{
				_target = _hit.point;
			}
			_item.position = Vector3.SmoothDamp(_item.position, _target, ref _smootVector, _smootTime);
		}

		public override void finish()
		{
			if (_view != null)
			{
				AbstractView view = _view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
			}
			if (!finished)
			{
				base.finish();
				if ((bool)_item)
				{
					UnityEngine.Object.Destroy(_item.gameObject);
				}
			}
		}
	}
}
