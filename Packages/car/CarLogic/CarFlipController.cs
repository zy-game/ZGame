using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	internal class CarFlipController : ControllerBase, IResetChecker
	{
		private Transform transform;

		private Rigidbody rigidbody;

		private AbstractView view;

		private Vector3 tmpVector = Vector3.zero;

		private CarState carState;

		public float ResetDelay => 0.6f;

		public object ResetUserData => null;

		internal CarFlipController(CarState state)
		{
			carState = state;
			transform = state.transform;
			rigidbody = carState.rigidbody;
			view = state.view;
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
		}

		public bool StartToWait()
		{
			if (base.Active && carState.LinearVelocity < 0.3f && carState.GroundHit == 0)
			{
				return true;
			}
			return false;
		}

		public bool NeedReset(object data)
		{
			return StartToWait();
		}

		public bool Cancelable(object data)
		{
			return false;
		}

		public void OnReset()
		{
			Vector3 lastNormal = carState.LastNormal;
			transform.rotation = Quaternion.LookRotation(transform.forward, lastNormal);
			transform.position += lastNormal * 0.5f;
			if ((bool)rigidbody && !rigidbody.isKinematic)
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}
		}
	}
}
