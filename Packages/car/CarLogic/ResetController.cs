using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	internal class ResetController : ControllerBase
	{
		private List<IResetChecker> checkerList = new List<IResetChecker>(4);

		[SerializeField]
		private bool waitForReset;

		private CarState carState;

		private AbstractView view;

		private LinkedList<ResetWaiter> waitList = new LinkedList<ResetWaiter>();

		private float resetSleepTime = 0.2f;

		private bool sleeping;

		private bool _isReseting;

		private float _resetingStartTime;

		private float _resetingHoldTime = 0.5f;

		public bool IsReseting => _isReseting;

		public void Init(CarState state, AbstractView view)
		{
			carState = state;
			this.view = view;
		}

		internal void AddChecker(IResetChecker checker)
		{
			if (!checkerList.Contains(checker))
			{
				checkerList.Add(checker);
			}
		}

		internal void RemoveChecker(IResetChecker checker)
		{
			checkerList.Remove(checker);
		}

		public override void OnActiveChange(bool active)
		{
			AbstractView abstractView = view;
			abstractView.OnUpdate = (Action)Delegate.Remove(abstractView.OnUpdate, new Action(update));
			if (active)
			{
				AbstractView abstractView2 = view;
				abstractView2.OnUpdate = (Action)Delegate.Combine(abstractView2.OnUpdate, new Action(update));
			}
			StopAllReset();
		}

		private void update()
		{
			if (!base.Active)
			{
				return;
			}
			if (!sleeping)
			{
				for (int i = 0; i < checkerList.Count; i++)
				{
					IResetChecker resetChecker = checkerList[i];
					if (resetChecker.StartToWait())
					{
						sleeping = true;
						view.CallDelay(delegate
						{
							sleeping = false;
						}, resetSleepTime);
						waitList.AddLast(new ResetWaiter(resetChecker, Time.time + resetChecker.ResetDelay, resetChecker.ResetUserData));
					}
				}
			}
			LinkedListNode<ResetWaiter> linkedListNode = waitList.First;
			waitForReset = linkedListNode != null;
			float time = Time.time;
			while (linkedListNode != null)
			{
				if (time > linkedListNode.Value.ResetTime)
				{
					waitList.Remove(linkedListNode);
					ResetDelay(linkedListNode.Value.Checker, linkedListNode.Value.UserData);
					break;
				}
				if (linkedListNode.Value.Checker.Cancelable(linkedListNode.Value.UserData))
				{
					waitList.Remove(linkedListNode);
				}
				linkedListNode = linkedListNode.Next;
			}
			if (_isReseting && Time.time - _resetingStartTime > _resetingHoldTime)
			{
				CheckGroundHit();
			}
		}

		private void CheckGroundHit()
		{
			if (carState.GroundHit != 0 && _isReseting)
			{
				_isReseting = false;
			}
		}

		private void delWaiter(IResetChecker chk)
		{
			ResetWaiter resetWaiter = null;
			foreach (ResetWaiter wait in waitList)
			{
				if (chk == wait.Checker)
				{
					resetWaiter = wait;
					break;
				}
			}
			if (resetWaiter != null)
			{
				waitList.Remove(resetWaiter);
			}
		}

		internal void ResetDelay(IResetChecker checker, object userData = null)
		{
			float time = Time.time;
			if (Reset(checker, userData))
			{
				StopAllReset();
				sleeping = false;
			}
		}

		internal void StopAllReset()
		{
			waitList.Clear();
		}

		internal bool Reset(IResetChecker checker, object userData = null)
		{
			if (!base.Active)
			{
				return false;
			}
			if (carState == null)
			{
				return false;
			}
			if (checker.NeedReset(userData))
			{
				ResetImmediately(checker);
				return true;
			}
			return false;
		}

		internal void ResetImmediately(IResetChecker checker)
		{
			if (base.Active && carState != null && checker != null)
			{
				_isReseting = true;
				_resetingStartTime = Time.time;
				Debug.LogWarning(checker.ToString() + " Reset car on " + Time.time);
				checker.OnReset();
				carState.CollisionDirection = HitWallDirection.NONE;
				if ((bool)carState.rigidbody && !carState.rigidbody.isKinematic)
				{
					carState.rigidbody.velocity = Vector3.zero;
					carState.rigidbody.angularVelocity = Vector3.zero;
				}
				carState.CurDriftState.Stage = DriftStage.NONE;
				carState.Drift = false;
				carState.view.ItController.StopAllGasItems();
				carState.view.crasherController.OnReset();
				carState.view.StartFlash();
				carState.view.SkController.ResetAllSkidmarks();
			}
		}
	}
}
