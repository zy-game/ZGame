using System;
using UnityEngine;

namespace CarLogic
{
	public class ItemNodeTranslator : ItemTweener
	{
		protected float _flySpeed;

		protected float _rotateRatio;

		protected Transform _controlObj;

		protected RacePathNode _curNode;

		protected RacePathNode _leftNode;

		protected Ray _tmpRay;

		protected RaycastHit _tmpHit;

		protected Vector3 _tmpUp = Vector3.up;

		public float FlySpeed
		{
			get
			{
				return _flySpeed;
			}
			set
			{
				_flySpeed = value;
			}
		}

		public ItemNodeTranslator(CarState target, Transform controlObj, float flySpeed, float rotateRatio, float duration = 3f, Action callback = null)
			: base(target, duration, callback)
		{
			_controlObj = controlObj;
			_flySpeed = flySpeed;
			_rotateRatio = rotateRatio;
		}

		public void UpdatePos()
		{
			if (Time.realtimeSinceStartup - startTime > maxDuration)
			{
				finish();
				return;
			}
			_controlObj.position += _controlObj.transform.forward * _flySpeed * Time.fixedDeltaTime;
			_tmpRay.origin = _controlObj.position + _controlObj.up * 5f;
			_tmpRay.direction = -_controlObj.up;
			if (Physics.Raycast(_tmpRay, out _tmpHit, 10f, LayerMask.GetMask("Road")))
			{
				_controlObj.position = _tmpHit.point;
			}
			_controlObj.rotation = Quaternion.LookRotation(calForward(_controlObj.position));
		}

		public override void Start()
		{
			base.Start();
			startTime = Time.realtimeSinceStartup;
			AbstractView abstractView = _controlObj.gameObject.GetComponent<AbstractView>();
			if (abstractView == null)
			{
				abstractView = _controlObj.gameObject.AddComponent<AbstractView>();
			}
			AbstractView abstractView2 = abstractView;
			abstractView2.OnFixedupdate = (Action)Delegate.Combine(abstractView2.OnFixedupdate, new Action(UpdatePos));
			_curNode = target.CurNode;
			_leftNode = target.CurNode.LeftNode;
			if (_leftNode != null)
			{
				Vector3 onNormal = _leftNode.transform.position - _curNode.transform.position;
				Vector3 vector = target.transform.position - _curNode.transform.position;
				Vector3 vector2 = Vector3.Project(vector, onNormal);
				_controlObj.position = vector2 + _curNode.transform.position;
				_controlObj.rotation = Quaternion.LookRotation(onNormal.normalized);
			}
			else
			{
				_controlObj.position = target.transform.position;
				_controlObj.rotation = target.transform.rotation;
			}
		}

		protected Vector3 calForward(Vector3 position)
		{
			if (_leftNode == null)
			{
				return _curNode.transform.forward;
			}
			if (_leftNode.LeftNode == null)
			{
				return _leftNode.transform.position - _curNode.transform.position;
			}
			Vector3 position2 = _curNode.transform.position;
			Vector3 position3 = _leftNode.transform.position;
			Vector3 position4 = _leftNode.LeftNode.transform.position;
			Vector3 vector = position3 - position2;
			Vector3 b = position4 - position3;
			Vector3 onNormal = position3 - position2;
			Vector3 vector2 = position - position2;
			Vector3 vector3 = Vector3.Project(vector2, onNormal);
			float num = 0f;
			num = ((vector3.x != 0f) ? (vector3.x / onNormal.x) : ((vector3.y != 0f) ? (vector3.y / onNormal.y) : ((vector3.z == 0f) ? 0f : (vector3.z / onNormal.z))));
			if (num >= 1f)
			{
				_curNode = _leftNode;
				_leftNode = _leftNode.LeftNode;
			}
			if (_rotateRatio >= 1f)
			{
				return vector;
			}
			return Vector3.Slerp(vector, b, Mathf.Max(0f, num - _rotateRatio) / (1f - _rotateRatio));
		}

		public override void finish()
		{
			base.finish();
			AbstractView component = _controlObj.gameObject.GetComponent<AbstractView>();
			if (component != null)
			{
				component.OnUpdate = (Action)Delegate.Remove(component.OnUpdate, new Action(UpdatePos));
			}
		}
	}
}
