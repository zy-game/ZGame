using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class SyncMoveController : ControllerBase, IResetChecker
	{
		public static Bool OpenNewSyncMove = true;

		public static Bool OpenNewCollision = true;

		public static Bool OpenNewForeCast = false;

		public static Bool OpenNewForecastOnlybyMoveMsg = false;

		private SyncMoveControllerClassic controlClassic;

		private SyncMoveControllerNew controlNew;

		private SyncMoveControllerForecast controlForecast;

		public new bool Active
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.Active;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.Active;
				}
				return controlNew.Active;
			}
			set
			{
				if (controlClassic != null)
				{
					controlClassic.Active = value;
				}
				else if ((bool)OpenNewForeCast)
				{
					controlForecast.Active = value;
				}
				else
				{
					controlNew.Active = value;
				}
			}
		}

		public Action<byte[]> OnSelfCarSendAction
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.OnSelfCarSendAction;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.OnSelfCarSendAction;
				}
				return controlNew.OnSelfCarSendAction;
			}
			set
			{
				if (controlClassic != null)
				{
					controlClassic.OnSelfCarSendAction = value;
				}
				else if ((bool)OpenNewForeCast)
				{
					controlForecast.OnSelfCarSendAction = value;
				}
				else
				{
					controlNew.OnSelfCarSendAction = value;
				}
			}
		}

		public float AsyncInterval
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.AsyncInterval;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.SyncMoveSender.AsyncInterval;
				}
				return controlNew.SyncMoveSender.AsyncInterval;
			}
			set
			{
				if (controlClassic != null)
				{
					controlClassic.AsyncInterval = value;
				}
				else if ((bool)OpenNewForeCast)
				{
					controlForecast.SyncMoveSender.AsyncInterval = value;
				}
				else
				{
					controlNew.SyncMoveSender.AsyncInterval = value;
				}
			}
		}

		public long NetDelay
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.NetDelay;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.SyncMoveSender.NetDelay;
				}
				return controlNew.SyncMoveSender.NetDelay;
			}
			set
			{
				if (controlClassic != null)
				{
					controlClassic.NetDelay = value;
				}
				else if ((bool)OpenNewForeCast)
				{
					controlForecast.SyncMoveSender.NetDelay = value;
				}
				else
				{
					controlNew.SyncMoveSender.NetDelay = value;
				}
			}
		}

		public float ResetDelay
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.ResetDelay;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.ResetDelay;
				}
				return controlNew.ResetDelay;
			}
		}

		public object ResetUserData
		{
			get
			{
				if (controlClassic != null)
				{
					return controlClassic.ResetUserData;
				}
				if ((bool)OpenNewForeCast)
				{
					return controlForecast.ResetUserData;
				}
				return controlNew.ResetUserData;
			}
		}

		public SyncMoveController()
		{
			controlClassic = null;
			controlNew = null;
			if ((bool)OpenNewSyncMove)
			{
				if ((bool)OpenNewForeCast)
				{
					controlForecast = new SyncMoveControllerForecast();
				}
				else
				{
					controlNew = new SyncMoveControllerNew();
				}
			}
			else
			{
				controlClassic = new SyncMoveControllerClassic();
			}
		}

		public void Init(CarView view, CarState state, CarModel model)
		{
			if (controlClassic != null)
			{
				controlClassic.Init(view, state, model);
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.Init(view, state, model);
			}
			else
			{
				controlNew.Init(view, state, model);
			}
		}

		public int GetSyncMoveQueueCount()
		{
			if (controlClassic != null)
			{
				return controlClassic.GetSyncMoveQueueCount();
			}
			if ((bool)OpenNewForeCast)
			{
				return controlForecast.GetSyncMoveQueueCount();
			}
			return controlNew.GetSyncMoveQueueCount();
		}

		public void ClearSyncMoveQueue()
		{
			if (controlClassic != null)
			{
				controlClassic.ClearSyncMoveQueue();
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.ClearSyncMoveQueue();
			}
			else
			{
				controlNew.ClearSyncMoveQueue();
			}
		}

		public void AsyncMovement(byte[] bytes, bool forceSet)
		{
			if (controlClassic != null)
			{
				controlClassic.AsyncMovement(bytes, forceSet);
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.AsyncMovement(bytes, forceSet);
			}
			else
			{
				controlNew.AsyncMovement(bytes, forceSet);
			}
		}

		public void AsyncMovement(MovementData data, bool forceSet)
		{
			if (controlClassic != null)
			{
				controlClassic.AsyncMovement(data, forceSet);
			}
			else if ((bool)OpenNewForeCast)
			{
				Debug.LogError("没有实现接口");
			}
			else
			{
				controlNew.AsyncMovement(data, forceSet);
			}
		}

		public void SendTransform()
		{
			if (controlClassic != null)
			{
				controlClassic.SendTransform();
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.SyncMoveSender.SendTransform();
			}
			else
			{
				controlNew.SyncMoveSender.SendTransform();
			}
		}

		public void ResetTimeLeft()
		{
			if (controlClassic != null)
			{
				controlClassic.ResetTimeLeft();
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.SyncMoveSender.ResetTimeLeft();
			}
			else
			{
				controlNew.SyncMoveSender.ResetTimeLeft();
			}
		}

		public bool StartToWait()
		{
			if (controlClassic != null)
			{
				return controlClassic.StartToWait();
			}
			if ((bool)OpenNewForeCast)
			{
				return controlForecast.StartToWait();
			}
			return controlNew.StartToWait();
		}

		public bool NeedReset(object data)
		{
			if (controlClassic != null)
			{
				return controlClassic.NeedReset(data);
			}
			if ((bool)OpenNewForeCast)
			{
				return controlForecast.NeedReset(data);
			}
			return controlNew.NeedReset(data);
		}

		public bool Cancelable(object data)
		{
			if (controlClassic != null)
			{
				return controlClassic.Cancelable(data);
			}
			if ((bool)OpenNewForeCast)
			{
				return controlForecast.Cancelable(data);
			}
			return controlNew.Cancelable(data);
		}

		public void OnReset()
		{
			if (controlClassic != null)
			{
				controlClassic.OnReset();
			}
			else if ((bool)OpenNewForeCast)
			{
				controlForecast.OnReset();
			}
			else
			{
				controlNew.OnReset();
			}
		}
	}
}
