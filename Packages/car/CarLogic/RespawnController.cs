using System;
using UnityEngine;

namespace CarLogic
{
	internal class RespawnController : ControllerBase, IResetChecker
	{
		private CarState carState;

		private bool fallDown;

		public float ResetDelay => 0f;

		public object ResetUserData => null;

		internal RespawnController()
		{
		}

		internal RespawnController(CarState state)
		{
			carState = state;
		}

		public void SetCarState(CarState state)
		{
			carState = state;
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			CarView view = carState.view;
			view.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
			if (active)
			{
				CarView view2 = carState.view;
				view2.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view2.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
			}
		}

		private void onTriggerSpecial(Collider c)
		{
			int layer = c.gameObject.layer;
			if (layer == 24)
			{
				fallDown = true;
			}
		}

		public bool StartToWait()
		{
			if (!base.Active)
			{
				return false;
			}
			if (carState == null || carState.rigidbody == null)
			{
				return false;
			}
			if (fallDown)
			{
				return true;
			}
			float y = carState.rigidbody.position.y;
			if (y < RacePathManager.ResetLowLimit || y > 200f + RacePathManager.ResetLowLimit)
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
			fallDown = false;
			RacePathNode curNode = carState.CurNode;
			if (curNode == null)
			{
				LogWarning("No path node reached.");
				return;
			}
			Vector3 vector = curNode.transform.position;
			if (Physics.Raycast(vector + curNode.Up * 2f, -curNode.Up, out var hitInfo, 10f, 256))
			{
				vector = hitInfo.point;
			}
			else
			{
				Debug.LogWarning("No RoadPoint found at " + curNode.name);
			}
			vector += RaceConfig.ResetOffsetY * curNode.Up;
			carState.transform.position = vector;
			carState.transform.rotation = curNode.transform.rotation;
			if (carState.view.DriftEnable)
			{
				carState.view.DriftEnable = false;
				carState.view.DriftEnable = true;
			}
		}
	}
}
